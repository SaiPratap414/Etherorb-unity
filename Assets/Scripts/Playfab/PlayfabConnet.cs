using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayfabConnet : MonoBehaviour
{

    private string matchHistoryKey = "MatchHistrory";
    private string lastMatchIdKey = "LastMatchId";

    public static PlayfabConnet instance;
    [SerializeField] bool hasLogedIn = false;
    public bool GetHasLogedIn { get { return hasLogedIn; } }

    private string playerName;
    private string playFabId;
    public string PlayerName { get { return playerName; } }

    private string matchHistoriesData;
    public MatchHistory matchHistories = new MatchHistory();

    private Dictionary<string, List<Leaderboard>> playFabLeaderboardDictionary = new Dictionary<string, List<Leaderboard>>();

    public GameLeaderboardData gameLeaderboardData = new GameLeaderboardData();

    private string walletAdd;
    public int totalScore;

    [SerializeField] int numOfGamesPlayed = 0;

    [SerializeField] private GameConfig gameConfig;

    private EtherOrbManager etherOrbManager;

    public string lastMatchId { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        etherOrbManager = EtherOrbManager.Instance;
    }


    public void PlayFabLoginAfterSaving()
    {
        if (PlayerPrefs.HasKey("LoginId"))
        {

            walletAdd = PlayerPrefs.GetString("LoginId");

            Login();
        }
    }

    #region Login

    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = walletAdd,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailed);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success" + result.ToJson());

        // Save Login Id
        PlayerPrefs.SetString("LoginId", walletAdd);

        playFabId = result.PlayFabId;

        Debug.Log("playFabId--->" + playFabId);

        //UserPrefsManager.GetHasLogedIn = 1;

        GetPlayerData();
        //Check if it's first login
        //if (result.NewlyCreated)
        //{
        //    UpdateConsecutiveWins(false);
        //    UpdateGamesPlayed();
        //}
        //else
        //{
        //    // Fetch Player data from database on success
        //    GetPlayerData();
        //    GetStartingData();
        //}
    }

    private void OnLoginFailed(PlayFabError error)
    {
        Debug.LogError("Login Failed: " + error.ErrorMessage);
        //UserPrefsManager.GetHasLogedIn = 0;
    }

    public void OnLogOut()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        hasLogedIn = false;
        playerName = null;
        //UserPrefsManager.GetHasLogedIn = 0;
    }

    #endregion

    #region Player Data

    public void SetPlayerName(string _name)
    {
        playerName = _name;
        UpdatePlayerName();
        //MenuManager.instance.OpenMenuId(2);
    }

    private void UpdatePlayerName()
    {
        var nameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, OnDisplayNameUpdate, OnInvalidNameEnter);
    }


    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        UpdatePlayerData();
    }

    private void UpdatePlayerData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"PlayerName", playerName},
                {"ADAWalletAddress", walletAdd},
                {"TotalScore", totalScore.ToString()},
                {"GamesPlayed",  numOfGamesPlayed.ToString()},
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnDataError);
    }


    private void OnDataSend(UpdateUserDataResult result)
    {
        hasLogedIn = true;
        //MenuManager.instance.OpenMenuId(2);
        //PhotonConnector.instance.ConnectPhoton();
    }

    private void OnDataError(PlayFabError error)
    {
        Debug.LogError("Player Data Error: " + error.ErrorMessage);
        etherOrbManager.WarningPanel.ShowWarning(error.ErrorMessage);
    }
    private void OnInvalidNameEnter(PlayFabError error)
    {
        MenuManager.instance.OpenMenuId(1);
        etherOrbManager.WarningPanel.ShowWarning(error.ErrorMessage);
    }

    public void GetPlayerData()
    {
        Debug.Log("Trying to fetch player data");
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnDataError);
    }

    private void OnDataRecieved(GetUserDataResult result)
    {
        Debug.Log("Data Recieved");
        if (result == null || !result.Data.ContainsKey("ADAWalletAddress"))
        {

            MenuManager.instance.OpenMenuId(1);
            return;
        }

        playerName = result.Data["PlayerName"].Value;
        walletAdd = result.Data["ADAWalletAddress"].Value;
        
        if (result.Data.ContainsKey(matchHistoryKey))
        {
            matchHistories = null;
            matchHistoriesData = result.Data[matchHistoryKey].Value;
            matchHistories = JsonUtility.FromJson<MatchHistory>(matchHistoriesData);
            Debug.Log("User Match History Count ---> " +matchHistories.userMatchHistories.Count);
        }

        if (result.Data.ContainsKey(lastMatchIdKey))
            lastMatchId = result.Data[lastMatchIdKey].Value;

        int.TryParse(result.Data["TotalScore"].Value, out totalScore);
        string gamesPlayed = result.Data["GamesPlayed"].Value;
        numOfGamesPlayed = int.Parse(gamesPlayed);

        GetStartingData();
        hasLogedIn = true;
        // After Successful Login
        Debug.Log(playerName + " Player name.");
        Debug.Log(walletAdd + " walletAddress.");
        Debug.Log(totalScore + " TotalScore.");

        if (result.Data.ContainsKey(matchHistoryKey))
            Debug.Log(matchHistoriesData + " MatchHistrory.");

        MenuManager.instance.OpenMenuId(4);
        PhotonConnector.instance.ConnectPhoton();
    }

    public void addtoPlayerTotalScore(int score)
    {
        totalScore += score;
        UpdatePlayerScoreData();
    }

    void UpdatePlayerScoreData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"TotalScore",  totalScore.ToString()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnScoreChanged, OnDataError);
    }
    void OnScoreChanged(UpdateUserDataResult result)
    {
        Debug.Log("Changed Total Score To-  " + totalScore);
    }


    #region Update Games Played
    public void UpdateGamesPlayed()
    {
        numOfGamesPlayed += 1;

        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"GamesPlayed",  numOfGamesPlayed.ToString()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnUpdateGamesPlayed, OnDataError);
    }

    public void UpdatePlayerHistory(UserMatchHistory userMatchHistory)
    {
        matchHistories.userMatchHistories.Add(userMatchHistory);
        string userMatchHistoryJson = JsonUtility.ToJson(matchHistories);
        Debug.Log("userMatchHistoryJson----> " + userMatchHistoryJson);
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {matchHistoryKey,  userMatchHistoryJson}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnUpdateGamesPlayed, OnDataError);
    }

    public void UpdateLastMatchId(string lastMatchIdValue)
    {        
        Debug.Log("UpdateLastMatchId ----> " + lastMatchIdValue);
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {lastMatchIdKey,  lastMatchIdValue}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnUpdateGamesPlayed, OnDataError);
    }

    private void OnUpdateGamesPlayed(UpdateUserDataResult result)
    {
        Debug.Log("Updated Games Played data");
    }

    public void GetStartingData()
    {
        playFabLeaderboardDictionary.Clear();
        gameLeaderboardData.rankLeaderBoards.Clear();
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetStartingData, OnDataError);
        SendLeaderboard();
    }
    private void OnGetStartingData(GetUserDataResult result)
    {
        if (result != null)
        {

            if (result.Data.ContainsKey("GamesPlayed"))
            {
                string gamesPlayed = result.Data["GamesPlayed"].Value;
                numOfGamesPlayed = int.Parse(gamesPlayed);
            }

        }
    }
    #endregion

    #endregion
    // Not in Use.....
    #region Leaderboard

    public void SendLeaderboard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            CustomTags = new Dictionary<string, string>()
            {
                { "UserName", playerName },
                { "WalletID", string.Empty },
            },

        Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = gameConfig.statisticName[0],
                    Value = matchHistories.XP,
                },
                new StatisticUpdate
                {
                    StatisticName = gameConfig.statisticName[1],
                    Value = matchHistories.totalGamesPlayed,
                },
                new StatisticUpdate
                {
                    StatisticName = gameConfig.statisticName[2],
                    Value = (int)matchHistories.winRate,
                },
                 new StatisticUpdate
                {
                    StatisticName = gameConfig.statisticName[3],
                    Value = matchHistories.totalWinMatches,
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderboardError);
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Leaderboard Updated Successfully---->" + result.ToJson());
        GetLeaderboard();
    }
    private void OnLeaderboardError(PlayFabError error)
    {
        Debug.LogError("Leaderboard Error: " + error.ErrorMessage);
    }
    public void GetLeaderboard()
    {
        foreach (var item in gameConfig.statisticName)
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = item,
                StartPosition = 0,
                MaxResultsCount = 100
            };

            PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnLeaderboardError);
        }
    }
    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        Debug.LogError("GetLeaderboardResult : " + result.ToJson());
        LeaderboardModel leaderboardModel = new LeaderboardModel();
        leaderboardModel = JsonUtility.FromJson<LeaderboardModel>(result.ToJson());

        playFabLeaderboardDictionary.Add(leaderboardModel.Request.StatisticName, leaderboardModel.Leaderboard);

        if(playFabLeaderboardDictionary.Count == gameConfig.statisticName.Count)
        {
            SetRankDataForLeaderboard();
        }
    }

    private void SetRankDataForLeaderboard()
    {
        foreach (var item in playFabLeaderboardDictionary[gameConfig.statisticName[0]])
        {
            RankLeaderBoard rankLeaderBoard = new RankLeaderBoard();
            rankLeaderBoard.rank = item.Position+1;
            rankLeaderBoard.name = item.DisplayName;
            rankLeaderBoard.XP = item.StatValue;

            //Need to add winrate, gameplayed and vol...
            gameLeaderboardData.rankLeaderBoards.Add(rankLeaderBoard);
        }

        foreach (var item in playFabLeaderboardDictionary[gameConfig.statisticName[1]])
        {
            int index = gameLeaderboardData.rankLeaderBoards.FindIndex(x => x.name.Equals(item.DisplayName));
            if (index != -1)
            {
                gameLeaderboardData.rankLeaderBoards[index].gamePlayed = item.StatValue;
            }
        }

        foreach (var item in playFabLeaderboardDictionary[gameConfig.statisticName[2]])
        {
            int index = gameLeaderboardData.rankLeaderBoards.FindIndex(x => x.name.Equals(item.DisplayName));
            if (index != -1)
            {
                gameLeaderboardData.rankLeaderBoards[index].winRate = item.StatValue;
            }
        }
        foreach (var item in playFabLeaderboardDictionary[gameConfig.statisticName[3]])
        {
            int index = gameLeaderboardData.rankLeaderBoards.FindIndex(x => x.name.Equals(item.DisplayName));
            if (index != -1)
            {
                gameLeaderboardData.rankLeaderBoards[index].gameVol = item.StatValue;
            }
        }
    }

    public void FetchServerTime(Action<GetTimeResult> onSuccess,Action<PlayFabError> onError)
    {
        PlayFabClientAPI.GetTime(
            new GetTimeRequest(),
            onSuccess,
            onError
        );
    }
    #endregion

    #region UI

    public void PlayFabLoginWithWalletId(string _walletAdd)
    {
        walletAdd = _walletAdd;
        MenuManager.instance.OpenMenuId(4);
        Login();
    }

    #endregion
}
