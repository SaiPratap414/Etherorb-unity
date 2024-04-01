using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class PlayTrun
{
    // 1 = Terra , 2 = Torrent, 3 = Blaze  
    public int playerA;
    public int playerB;

    public PlayTrun()
    {
        playerA = 0;
        playerB = 0;
    }
    public bool isSameTurn()
    {
        return (playerA == playerB);
    }
    /// <summary>
    /// Determines The Winner
    /// </summary>
    /// <returns> 1 if PlayerA Won and 2 if PlayerB Won</returns>
    public int GetWhoWon()
    {
        if (playerA == 1 & playerB == 2)
            return 1;
        else if (playerA == 2 & playerB == 3)
            return 1;
        else if (playerA == 3 & playerB == 1)
            return 1;
        else 
            return 2; // Player B Won.(meaning PlayerA lost)
    }

}

[Serializable]
public class PlayerPoints
{
    public int playerAPoints;
    public int playerBPoints;
    public PlayerPoints()
    {
        playerAPoints = 0;
        playerBPoints = 0;
    }
}


public enum GameState
{
    Nothing,
    Roundstart,
    RoundWaitInput,
    RoundEnd,
    waitingForResults,
    GameEnd,
}


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] GameState gameState;
    public GameState setGameState { set { gameState = value; } }

    bool gameStarted = false;
    bool gameCompleted = false;

    [SerializeField] OrbGameUI playerOrb1;
    public OrbGameUI GetPlayerOrb1 {  get { return playerOrb1; } }
    [SerializeField] OrbGameUI playerOrb2;
    public OrbGameUI GetPlayerOrb2 { get { return playerOrb2; } }


    [Header("UI Elements")]
    [SerializeField] TMP_Text[] UNames = new TMP_Text[2];
    [SerializeField] TMP_Text[] UScores = new TMP_Text[2];
    [SerializeField] Button[] Optionbuttons = new Button[3];
    [SerializeField] GameObject roundHistoryPlayerA;
    [SerializeField] GameObject roundHistoryPlayerB;
    [SerializeField] GameObject lastPlayUiPrefab;
    [SerializeField] TMP_Text roundNumText;
    [SerializeField] TMP_Text headingText;
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] GameObject GameClosePanel;
    [SerializeField] GameObject VictoryPanel;
    [SerializeField] GameObject DefeatPanel;
    [SerializeField] GameObject DrawPanel;
    [SerializeField] TMP_Text EndPanelScore;
    [SerializeField] TMP_Text EndPanelHeader;
    [SerializeField] Menu LoadingScreen;

    [SerializeField] Sprite WonSprite;
    [SerializeField] Sprite LostSprite;


    public Button[] GetOptionButtons { get { return Optionbuttons; } }

    public Player[] players = null;
    public PlayerManager playerManager1 = null;
    public PlayerManager playerManager2 = null;
    bool isPlayer1R = false;
    bool isPlayer2R = false;


    [Header(" ")]
    public PlayTrun currentPlay = new PlayTrun();
    [SerializeField] int[] CurrentRoundScore = new int[2];
    PlayTrun RandomPlay = new PlayTrun();

    public List<PlayTrun> previousTurns = new List<PlayTrun>();

    [SerializeField] int roundNum = 0;
    [SerializeField] int maxRoundNum = 0;

    [Header("Points / Score")]

    [SerializeField] int playerAScore;
    [SerializeField] int playerBScore;
    RoundTimer rTimer;

    PhotonView pv;


    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        rTimer = GetComponent<RoundTimer>();
        players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log(players[i].NickName + " Actor name, " + players[i].ActorNumber + " Actor number");
            UNames[i].text = players[i].NickName;
        }
        roundNumText.text = "Round " + roundNum;
        UScores[0].text = "0";
        UScores[1].text = "0";
        gameState = GameState.Nothing;
    }


    private void Update()
    {
        if (!gameStarted && isPlayer1R && isPlayer2R)
        {
            gameState = GameState.Roundstart;
            SyncPlayerData();
            LoadingScreen.Close();
            gameStarted = true;
        }

        switch (gameState)
        {
            case GameState.Roundstart:
                StartCoroutine(nameof(StartNewRound));
                break;
            case GameState.RoundWaitInput:
                CheckInput();

                break;
            case GameState.RoundEnd:
                StartCoroutine(nameof(CalculatePlay));
                break;
            case GameState.waitingForResults:
                waitinForResults();
                break;
            case GameState.GameEnd:
                StartCoroutine(nameof(GameEndSeq));
                break;
            case GameState.Nothing:

                break;
            default:
                break;
        }
    }

    void CheckInput()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            if (currentPlay.playerA != 0)
            {
                
            }
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            if (currentPlay.playerB != 0)
            {
               
            }
        }
        if (currentPlay.playerA != 0 && currentPlay.playerB != 0)
        {
            gameState = GameState.RoundEnd;
            rTimer.StopTimer();
        }
    }

    public void InputChoise(int choice, int ActorNumber)
    {
        if (ActorNumber == 1) currentPlay.playerA = choice;
        if (ActorNumber == 2) currentPlay.playerB = choice;
    }


    IEnumerator CalculatePlay()
    {
        gameState = GameState.Nothing;
        ButtonsVisible(false);

        yield return new WaitForSeconds(1.2f);
        GenerateRandomOption();
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_CalculatePlay), RpcTarget.AllBuffered);

        yield return new WaitForSeconds(0.5f);
        ResetRound();

    }

    [PunRPC]
    void RPC_CalculatePlay()
    {
        bool PlayerAWon = false;
        if (currentPlay.playerA == 0 || currentPlay.playerB == 0)
        {
            Debug.LogWarning("Calculate Play is being called");
            if (currentPlay.playerA == 0)
            {
                currentPlay.playerA = RandomPlay.playerA; // 1-3
            }

            if (currentPlay.playerB == 0)
            {
                currentPlay.playerB = RandomPlay.playerB; // 1-3
            }
        }

        if(currentPlay.isSameTurn())
        {
            if(currentPlay.playerA == 1)
            {
                // Check Terra Power Value
                if(playerManager1.TerraValue > playerManager2.TerraValue)
                {
                    playerAScore += 1;
                    Debug.LogWarning("Player A Won");
                    PlayerAWon = true; 
                }
                else if(playerManager1.TerraValue < playerManager2.TerraValue)
                {
                    playerBScore += 1;
                    Debug.LogWarning("Player B Won");
                    PlayerAWon = false;
                }
                else
                {
                    Debug.LogWarning("Edge Case Both have same Value");
                }

            }
            if (currentPlay.playerA == 2)
            {
                if (playerManager1.TorrentValue > playerManager2.TorrentValue)
                {
                    playerAScore += 1;
                    Debug.LogWarning("Player A Won");
                    PlayerAWon = true;
                }
                else if (playerManager1.TorrentValue < playerManager2.TorrentValue)
                {
                    playerBScore += 1;
                    Debug.LogWarning("Player B Won");
                    PlayerAWon = false;
                }
                else
                {
                    Debug.LogWarning("Edge Case Both have same Value");
                }
            }
            if (currentPlay.playerA == 3)
            {
                if (playerManager1.BlazeValue > playerManager2.BlazeValue)
                {
                    playerAScore += 1;
                    Debug.LogWarning("Player A Won");
                    PlayerAWon = true;
                }
                else if (playerManager1.BlazeValue < playerManager2.BlazeValue)
                {
                    playerBScore += 1;
                    Debug.LogWarning("Player B Won");
                    PlayerAWon = false;
                }
                else
                {
                    Debug.LogWarning("Edge Case Both have same Value");
                }
            }
        }
        else
        {
            int whoWon = currentPlay.GetWhoWon();

            if (whoWon == 1)
            {
                playerAScore += 1;
                Debug.LogWarning("Player A Won");
                PlayerAWon = true;
            }
            else if (whoWon == 2)
            {
                playerBScore += 1;
                Debug.LogWarning("Player B Won");
                PlayerAWon = false;
            }
        }
        

        UScores[0].text = playerAScore.ToString("0");
        UScores[1].text = playerBScore.ToString("0");

        spawnlastPlayUi(PlayerAWon);
    }



    void ResetRound()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_ResetRound), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetRound()
    {
        previousTurns.Add(currentPlay);
        currentPlay = new PlayTrun();
        gameState = GameState.waitingForResults;
    }

    IEnumerator StartNewRound()
    {
        gameState = GameState.Nothing;
        yield return new WaitForSeconds(0.8f);
        rTimer.resetTimer();
        rTimer.setRoundBool(true);
        headingText.text = " Select Input ";
        ButtonsVisible(true);
        gameState = GameState.RoundWaitInput;

    }

    void waitinForResults()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_waitingForResult), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_waitingForResult()
    {
        StartCoroutine(nameof(ImwaitinForResults));
    }

    IEnumerator ImwaitinForResults()
    {
        gameState = GameState.Nothing;
        yield return new WaitForSeconds(2f);
        roundNum++;
        if(playerAScore < 3 && playerBScore < 3)
        {
            roundNumText.text = "Round " + roundNum;
            gameState = GameState.Roundstart;
        }
        else
        {
            gameState = GameState.GameEnd;
        }
    }

    IEnumerator GameEndSeq()
    {
        gameState = GameState.Nothing;
        gameCompleted = true;
        yield return new WaitForSeconds(1.2f);
        GameOverPanel.SetActive(true);
        CalculateWinLoss();
        PlayfabConnet.instance.UpdateGamesPlayed();
        PhotonNetwork.LeaveRoom();
    }


    void CalculateWinLoss()
    {
        // Condition not in use ............... But might come handy.
        if (playerAScore == playerBScore)
        {
            sendScoreForPlayers(5, 5);
            EndPanelScore.text = GetPlayerScore(pv.OwnerActorNr).ToString("0");
            DrawPanel.SetActive(true);
            return;
        }
        //

        bool playerAWon = playerAScore > playerBScore ? true : false;
        if (playerAWon)
        {
            sendScoreForPlayers(10, 2);
        }
        else
        {
            sendScoreForPlayers(2, 10);
        }

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            EndPanelScore.text = GetPlayerScore(1).ToString("0");
            VictoryPanel.SetActive(playerAWon);
            DefeatPanel.SetActive(!playerAWon);
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            EndPanelScore.text = GetPlayerScore(2).ToString("0");
            VictoryPanel.SetActive(!playerAWon);
            DefeatPanel.SetActive(playerAWon);
        }
    }

    void ButtonsVisible(bool isVisibl)
    {
        Optionbuttons[0].gameObject.SetActive(isVisibl);
        Optionbuttons[1].gameObject.SetActive(isVisibl);
        Optionbuttons[2].gameObject.SetActive(isVisibl);
    }


    void spawnlastPlayUi(bool playerAWon)
    {
        GameObject uiA = Instantiate(lastPlayUiPrefab, roundHistoryPlayerA.transform);
        uiA.transform.GetChild(0).GetComponent<Image>().sprite = playerAWon ? WonSprite : LostSprite;
        GameObject uiB = Instantiate(lastPlayUiPrefab, roundHistoryPlayerB.transform);
        uiB.transform.GetChild(0).GetComponent<Image>().sprite = playerAWon ? LostSprite : WonSprite;
    }



    public void LeaveGame()
    {
        PhotonNetwork.LoadLevel(0);
    }

    public void GameFailedExit()
    {
        if (gameCompleted)
            return;
        gameState = GameState.Nothing;
        rTimer.StopTimer();
        Debug.Log("Game has ended Because a player left the game in middile..");
        GameOverPanel.SetActive(true);
        EndPanelHeader.text = "Game has ended Because a player left the game in middile...";
        EndPanelScore.gameObject.SetActive(false);
    }



    public int GetPlayerScore(int player)
    {
        if (player == 1)
        {
            return playerAScore;
        }
        else
        {
            return playerBScore;
        }
    }

    void sendScoreForPlayers(int player1, int player2)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            PlayfabConnet.instance.addtoPlayerTotalScore(player1);
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            PlayfabConnet.instance.addtoPlayerTotalScore(player2);
        }
    }


    void GenerateRandomOption()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        int RandomA = UnityEngine.Random.Range(1, 4);
        int RandomB = UnityEngine.Random.Range(1, 4);
        Debug.Log("Random Generated: A-" + RandomA + "B-" + RandomB);
        pv.RPC(nameof(RPC_StoreRandomOption), RpcTarget.All, RandomA, RandomB);

    }

    [PunRPC]
    void RPC_StoreRandomOption(int RandomA, int RandomB)
    {
        RandomPlay.playerA = RandomA; 
        RandomPlay.playerB = RandomB;
    }

    void SyncPlayerData()
    {
        playerOrb1.SetOrbStats(playerManager1.getOrbDetails);
        playerOrb2.SetOrbStats(playerManager2.getOrbDetails);
    }

    public void SetPlayerReady(int playerNum)
    {
        if(playerNum == 1)
            isPlayer1R = true;
        if(playerNum == 2)
            isPlayer2R = true;
    }

}
