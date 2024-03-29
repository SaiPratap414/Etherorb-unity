using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfabConnet : MonoBehaviour
{
    public static PlayfabConnet instance;
    [SerializeField] bool hasLogedIn = false;
    public bool GetHasLogedIn { get { return hasLogedIn; } }

    private string playerName;
    public string PlayerName { get { return playerName; } }

    private string walletAdd;
    public int totalScore;

    [SerializeField] int numOfGamesPlayed = 0;


    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

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
        Debug.Log("Login Success");

        // Save Login Id
        PlayerPrefs.SetString("LoginId", walletAdd);

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
    }

    public void OnLogOut()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        hasLogedIn = false;
        playerName = null;
    }

    #endregion

    #region Player Data

    public void SetPlayerName(string _name)
    {
        playerName = _name;
        UpdatePlayerName();
        MenuManager.instance.OpenMenuId(2);
    }

    private void UpdatePlayerName()
    {
        var nameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, OnDisplayNameUpdate, OnDataError);
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
        PhotonConnector.instance.ConnectPhoton();
    }

    private void OnDataError(PlayFabError error)
    {

        Debug.LogError("Player Data Error: " + error.ErrorMessage);
        MenuManager.instance.OpenMenuId(0);
    }

    private void GetPlayerData()
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
        int.TryParse(result.Data["TotalScore"].Value, out totalScore);

        string gamesPlayed = result.Data["GamesPlayed"].Value;
        numOfGamesPlayed = int.Parse(gamesPlayed);

        GetStartingData();
        hasLogedIn = true;
        // After Successful Login
        Debug.Log(playerName + " Player name.");
        Debug.Log(walletAdd + " walletAddress.");
        Debug.Log(totalScore + " TotalScore.");
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
        SendLeaderboard(totalScore);
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

    private void OnUpdateGamesPlayed(UpdateUserDataResult result)
    {
        Debug.Log("Updated Games Played data");
    }

    public void GetStartingData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetStartingData, OnDataError);
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

    public void SendLeaderboard(int _score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "HighScore",
                    Value = _score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderboardError);
    }


    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Leaderboard Updated Successfully");
    }

    private void OnLeaderboardError(PlayFabError error)
    {
        Debug.LogError("Leaderboard Error: " + error.ErrorMessage);
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 100
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnLeaderboardError);

    }

    public void GetLeaderboardAroundPlayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "HighScore",
            MaxResultsCount = 100,
        };


        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardGetPlayer, OnLeaderboardError);
    }

    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        
    }

    private void OnLeaderboardGetPlayer(GetLeaderboardAroundPlayerResult result)
    {

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
