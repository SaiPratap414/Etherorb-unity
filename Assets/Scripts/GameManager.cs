using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[Serializable]
public class PlayTrun
{
    public int playerA;
    public int playerB;

    public PlayTrun()
    {
        playerA = 0;
        playerB = 0;
    }
    public static bool operator ==(PlayTrun p1, PlayTrun p2)
    {
        return p1.playerA == p2.playerA && p1.playerB == p2.playerB;
    }
    public static bool operator !=(PlayTrun p1, PlayTrun p2)
    {
        return p1.playerA != p2.playerA && p1.playerB != p2.playerB;
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
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] GameObject GameClosePanel;
    [SerializeField] GameObject VictoryPanel;
    [SerializeField] GameObject DefeatPanel;
    [SerializeField] GameObject DrawPanel;
    [SerializeField] TMP_Text EndPanelScore;
    [SerializeField] TMP_Text EndPanelHeader;
    [SerializeField] Menu LoadingScreen;



    public Button[] GetOptionButtons { get { return Optionbuttons; } }

    public Player[] players = null;
    public PlayerManager playerManager1 = null;
    public PlayerManager playerManager2 = null;
    bool isPlayer1R = false;
    bool isPlayer2R = false;


    [Header(" ")]
    public PlayTrun currentPlay = new PlayTrun();
    [SerializeField] int[] CurrentRoundScore = new int[2];

    public List<PlayTrun> previousTurns = new List<PlayTrun>();

    [SerializeField] int roundNum = 0;
    [SerializeField] int maxRoundNum = 0;

    [Header("Points / Score")]

    [SerializeField] int playerAScore;
    [SerializeField] int playerBScore;
    [SerializeField] Image hintImage;
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

        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_CalculatePlay), RpcTarget.AllBuffered);

        yield return new WaitForSeconds(0.5f);
        ResetRound();

    }

    [PunRPC]
    void RPC_CalculatePlay()
    {
        PlayerPoints toCalculate = new PlayerPoints();
        if (currentPlay.playerA == 0 || currentPlay.playerB == 0)
        {
            if (currentPlay.playerA == 0 && currentPlay.playerB == 0)
            {
                //headingText.text = " Both Players have not selected any Option ";
            }
            else
            {
                //headingText.text = players[0].NickName + " has not selected any Option"
                if (currentPlay.playerA == 0)
                {
                }

                //headingText.text = players[1].NickName + " has not selected any Option"
                if (currentPlay.playerB == 0)
                {
                }
            }
        }

        playerAScore += toCalculate.playerAPoints;
        playerBScore += toCalculate.playerBPoints;

        CurrentRoundScore[0] = toCalculate.playerAPoints;
        CurrentRoundScore[1] = toCalculate.playerBPoints;

        UScores[0].text = playerAScore.ToString("0");
        UScores[1].text = playerBScore.ToString("0");


        //spawnlastPlayUi();
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
        //headingText.text = " Select Input ";
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
        hintImage.gameObject.SetActive(false);
        roundNum++;
        if (roundNum <= maxRoundNum)
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
        if (playerAScore == playerBScore)
        {
            sendScoreForPlayers(5, 5);
            EndPanelScore.text = GetPlayerScore(pv.OwnerActorNr).ToString("0");
            DrawPanel.SetActive(true);
            return;
        }
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


    void spawnlastPlayUi()
    {
        GameObject uiA = Instantiate(lastPlayUiPrefab, roundHistoryPlayerA.transform);
        uiA.transform.GetChild(0).GetComponent<Image>().sprite = getLastPlaySprite(currentPlay.playerA);
        uiA.transform.GetChild(1).GetComponent<TMP_Text>().text = CurrentRoundScore[0] > 0 ? "+" + CurrentRoundScore[0].ToString("0") : CurrentRoundScore[0].ToString("0");
        GameObject uiB = Instantiate(lastPlayUiPrefab, roundHistoryPlayerB.transform);
        uiB.transform.GetChild(0).GetComponent<Image>().sprite = getLastPlaySprite(currentPlay.playerB);
        uiB.transform.GetChild(1).GetComponent<TMP_Text>().text = CurrentRoundScore[1] > 0 ? "+" + CurrentRoundScore[1].ToString("0") : CurrentRoundScore[1].ToString("0");
    }

    Sprite getLastPlaySprite(int choice)
    {
        
        return null;
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
