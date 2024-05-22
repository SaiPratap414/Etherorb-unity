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

    [SerializeField] string[] animationStrings = new string[4];

    [Tooltip("In the Order of Terra Torrent Blaze")]
    [SerializeField] Button[] OptionbuttonsPlayer1 = new Button[4];
    [Tooltip("In the Order of Terra Torrent Blaze")]
    [SerializeField] Button[] OptionbuttonsPlayer2 = new Button[4];
    [SerializeField] GameObject roundHistoryPlayer;
    [SerializeField] GameObject elementGameobjectPlayerA;
    [SerializeField] GameObject elementGameobjectPlayerB;
    [SerializeField] GameObject staticUIPanel;
    [SerializeField] GameObject lastPlayUiPrefab;
    [SerializeField] TMP_Text roundNumText;

    [SerializeField] TextMeshProUGUI incrementScoreTextA;
    [SerializeField] TextMeshProUGUI incrementScoreTextB;

    [SerializeField] GameObject GameOverPanel;
    [SerializeField] GameObject GameClosePanel;
    [SerializeField] GameObject VictoryPanel;
    [SerializeField] GameObject DefeatPanel;
    [SerializeField] GameObject DrawPanel;
    [SerializeField] TMP_Text EndPanelScore;
    [SerializeField] TMP_Text EndPanelHeader;
    [SerializeField] Menu LoadingScreen;
    [SerializeField] Button homeButton;
    [SerializeField] Button reMatchButton;

    [Header("")]
    [SerializeField] TMP_Text AnnouncerHeader;
    [SerializeField] TMP_Text AnnouncerDesc;

    [SerializeField] Sprite WonSprite;
    [SerializeField] Sprite LostSprite;

    
    public Button[] GetOptionButtonsPlayer1 { get { return OptionbuttonsPlayer1; } }
    public Button[] GetOptionButtonsPlayer2 { get { return OptionbuttonsPlayer2; } }
    public string [] GetOrbAnimationName { get { return animationStrings; } }

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

    [SerializeField] private Button ruleButton;
    [SerializeField] private Button exitRulePanel;

    [SerializeField] private GameObject rulePanel;
    [SerializeField] private GameConfig gameConfig;

    [SerializeField] private Animator zapAnimatorA;
    [SerializeField] private Animator zapAnimatorB;

    [SerializeField] private Animator impactAnimatorA;
    [SerializeField] private Animator impactAnimatorB;

    public PhotonView pv;

    private CardType cardWon;
    private CardType cardLost;

    private AudioTag vfxTag;

    private AudioManager audioManager;
    private WarningPanel warningPanel;

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

        homeButton.onClick.AddListener(LeaveGame);
        reMatchButton.onClick.AddListener(SendRematchRequest);
    }
    private void Start()
    {
        warningPanel = EtherOrbManager.Instance.WarningPanel;
        audioManager = EtherOrbManager.Instance.AudioManager;
    }

    private void ResetUIForRematch()
    {
        roundNum = 0;
        roundNumText.text = "Round " + roundNum;
        UScores[0].text = "0";
        UScores[1].text = "0";
        gameState = GameState.Nothing;
        GameOverPanel.SetActive(false);
        playerAScore = playerBScore = 0;
        warningPanel.HideWarning();
        warningPanel.HidePopUp();
        foreach (Transform item in roundHistoryPlayer.transform)
        {
            Destroy(item.gameObject);
        }
    }

    public void ShowOrHideRulePanel(bool show)
    {
        rulePanel.SetActive(show);
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
            audioManager.StopTimerSound();
            rTimer.StopTimer();
        }
    }

    public void InputChoise(int choice, int ActorNumber)
    {
        if (ActorNumber == 1) currentPlay.playerA = choice;
        if (ActorNumber == 2) currentPlay.playerB = choice;

        if(currentPlay.playerA !=0 && currentPlay.playerB !=0)
        {
            SetParticleGameObject(GetOrbAnimationName[currentPlay.playerA], 1);
            SetParticleGameObject(GetOrbAnimationName[currentPlay.playerB], 2);
        }
    }


    IEnumerator CalculatePlay()
    {
        gameState = GameState.Nothing;
        ButtonsEnable(false);
        ShowOrDeSelectButtons(GetOptionButtonsPlayer1,false);
        ShowOrDeSelectButtons(GetOptionButtonsPlayer2, false);
        ClearUIForResult(false);
        yield return new WaitForSeconds(0.5f);

        GenerateRandomOption();
        yield return new WaitForSeconds(0.5f);
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_CalculatePlay), RpcTarget.AllBuffered);

        yield return new WaitForSeconds(2.4f);
        ResetRound();

    }

    [PunRPC]
    void RPC_CalculatePlay()
    {
        bool PlayerAWon = false;
        bool isDraw = false;
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
                    isDraw = true;
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
                    isDraw = true;
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
                    isDraw = true;
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


        UScores[0].text = playerAScore.ToString("00");
        UScores[1].text = playerBScore.ToString("00");

        SetCardWonAndLost(currentPlay);
        StartCoroutine(GameResultSequence(PlayerAWon, isDraw));
    }

    IEnumerator GameResultSequence(bool PlayerAWon, bool isDraw)
    {
        AnnouncerDesc.SetText(string.Empty);
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            if (PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorA, impactAnimatorB,playerOrb2));
            }
            else if(!PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorB, impactAnimatorA,playerOrb1));
            }
        }

        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            if (PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorA, impactAnimatorB,playerOrb2));
            }
            else if (!PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorB, impactAnimatorA,playerOrb1));
            }
        }

        spawnlastPlayUi(PlayerAWon, isDraw);
        SetAnnouncerHeader(PlayerAWon, isDraw);
        AnnouncerDesc.GetComponent<TextRevealEffect>().StartEffect(GenerateAnnouncerDescStringText(currentPlay));
    }

    private IEnumerator ExecuteAnimationSequence(Animator zap,Animator imapct, OrbGameUI orbGameUI)
    {
        zap.gameObject.SetActive(true);
        zap.enabled = true;
        WinVFXNames winVFX = gameConfig.winVFXNames.Find(x => x.type.Equals(cardWon));
        audioManager.PlayAudio(vfxTag);
        zap.Play(winVFX.zapAnimationName);
        yield return new WaitForSeconds(0.517f);
        zap.enabled = false;
        zap.gameObject.SetActive(false);

        imapct.gameObject.SetActive(true);
        imapct.enabled = true;
        imapct.Play(winVFX.impactAnimationName);
        LostAnimationName lostVFX = gameConfig.lostAnimationNames.Find(x => x.type.Equals(cardLost));
        orbGameUI.SetParticleGameObject(lostVFX.lostAnimationName);
        yield return new WaitForSeconds(0.517f);
        imapct.enabled = false;
        imapct.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
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
        audioManager.PlayAudio(AudioTag.Timer15);
        AnnouncerDesc.text = " Select Input ";
        ButtonsEnable(true);
        gameState = GameState.RoundWaitInput;

        AnnouncerHeader.text = string.Empty;
        // idle image...
        ResetOrbImages();

        ShowOrDeSelectButtons(GetOptionButtonsPlayer1);
        ShowOrDeSelectButtons(GetOptionButtonsPlayer2);
        ClearUIForResult(true);
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
        yield return new WaitForSeconds(1f);
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
        yield return new WaitForSeconds(1.5f);
        GameOverPanel.SetActive(true);
        CalculateWinLoss();
        PlayfabConnet.instance.UpdateGamesPlayed();
        //PhotonNetwork.LeaveRoom();
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
            AudioTag tag = playerAWon ? AudioTag.WinScreen : AudioTag.LoseScreen;
            audioManager.PlayAudio(tag);
            DefeatPanel.SetActive(!playerAWon);
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            EndPanelScore.text = GetPlayerScore(2).ToString("0");
            VictoryPanel.SetActive(!playerAWon);
            AudioTag tag = !playerAWon ? AudioTag.WinScreen : AudioTag.LoseScreen;
            audioManager.PlayAudio(tag);
            DefeatPanel.SetActive(playerAWon);
        }
    }

    void ButtonsEnable(bool isVisibl)
    {
        OptionbuttonsPlayer1[0].enabled = isVisibl;
        OptionbuttonsPlayer1[1].enabled = isVisibl;
        OptionbuttonsPlayer1[2].enabled = isVisibl;

        OptionbuttonsPlayer2[0].enabled = isVisibl;
        OptionbuttonsPlayer2[1].enabled = isVisibl;
        OptionbuttonsPlayer2[2].enabled = isVisibl;
    }


    void spawnlastPlayUi(bool playerAWon,bool isDraw)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1) // 1 is Player A
        {
            GameObject uiA = Instantiate(lastPlayUiPrefab, roundHistoryPlayer.transform);
            uiA.GetComponent<RoundUI>().SetUpRoundUI(playerAWon,isDraw);
            if (!isDraw)
            {
                if (playerAWon)
                {
                    incrementScoreTextA.gameObject.SetActive(playerAWon);
                    incrementScoreTextA.GetComponent<TextFadeInEffect>().ShowEffect(Color.white);
                }
                else
                {
                    incrementScoreTextB.gameObject.SetActive(!playerAWon);
                    incrementScoreTextB.GetComponent<TextFadeInEffect>().ShowEffect(Color.white);
                }
            }
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber == 2) 
        {
            GameObject uiB = Instantiate(lastPlayUiPrefab, roundHistoryPlayer.transform);
            uiB.GetComponent<RoundUI>().SetUpRoundUI(!playerAWon,isDraw);
            if (!isDraw)
            {
                if (!playerAWon)
                {
                    incrementScoreTextB.gameObject.SetActive(!playerAWon);
                    incrementScoreTextB.GetComponent<TextFadeInEffect>().ShowEffect(Color.white);
                }
                else
                {
                    incrementScoreTextA.gameObject.SetActive(playerAWon);
                    incrementScoreTextA.GetComponent<TextFadeInEffect>().ShowEffect(Color.white);
                }
            }
        }
    }


    public void SendRematchRequest()
    {
        pv.RPC(nameof(RPC_SendMatchRematchRequest), RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_SendMatchRematchRequest(int playernum)
    {
        reMatchButton.interactable = false;
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1) // 1 is Player A
        {
            if(playernum ==1)
            {
                //Show loading...
                warningPanel.ShowWarning("rematching to the player...", true);
            }
            else
            {
                // shop pop up
                warningPanel.ShowPopup();
            }
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2) // 1 is Player A
        {
            if(playernum==2)
            {
                warningPanel.ShowWarning("rematching to the player...", true);
            }
            else
            {
                // show pop up...
                warningPanel.ShowPopup();
            }
        }
    }
    public void ReMatchCancleRequest(int player)
    {
        pv.RPC(nameof(RPC_SendMatchRematchReject), RpcTarget.AllBuffered,player);
    }
    [PunRPC]
    void RPC_SendMatchRematchReject(int player)
    {
        reMatchButton.interactable = false;
        if (player == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            warningPanel.ShowWarning("Rematch Declined...");
        }
        else
        {
            warningPanel.HideWarning();
        }
    }
    public void ReMatchAcceptance()
    {
        pv.RPC(nameof(RPC_SendMatchRematchAccept), RpcTarget.AllBuffered);
    }
    [PunRPC]
    void RPC_SendMatchRematchAccept()
    {
        warningPanel.ShowWarning("Rematch Started...");
        //TODO ResetUI...
        ResetUIForRematch();
        ResetOrbImages();
        gameState = GameState.Roundstart;
    }

    public void LeaveGame()
    {
        audioManager.PlayAudio(AudioTag.Button);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }
    public void GameFailedExit()
    {
        if (gameCompleted)
            return;
        gameState = GameState.Nothing;
        rTimer.StopTimer();
        audioManager.StopTimerSound();
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
        if (currentPlay.playerA == 0 || currentPlay.playerB == 0)
        {
            int RandomA = UnityEngine.Random.Range(1, 4);
            int RandomB = UnityEngine.Random.Range(1, 4);
            Debug.Log("Random Generated: A-" + RandomA + "B-" + RandomB);
            pv.RPC(nameof(RPC_StoreRandomOption), RpcTarget.All, RandomA, RandomB);
            AnnouncerDesc.text = "NO Input Selected";
        }
    }

    [PunRPC]
    void RPC_StoreRandomOption(int RandomA, int RandomB)
    {
        
        RandomPlay.playerA = RandomA; 
        RandomPlay.playerB = RandomB;
        playerOrb1.SetParticleGameObject(GetOrbAnimationName[RandomA]);
        playerOrb2.SetParticleGameObject(GetOrbAnimationName[RandomB]);
    }

    void SyncPlayerData()
    {
        try
        {
            playerOrb1.SetOrbStats(playerManager1.getOrbDetails);
            playerOrb2.SetOrbStats(playerManager2.getOrbDetails);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void SetSelectedStatesForButton(int index,int player)
    {
        if(player ==1)
        {
            ShowOrDeSelectButtons(GetOptionButtonsPlayer1);
            GetOptionButtonsPlayer1[index - 1].GetComponent<ButtonUtility>().SelectedStateAction();
            string[] name = GetOptionButtonsPlayer1[index - 1].name.Split('_');
            AnnouncerDesc.text = name.Length > 0 ? name[0] + " Selected" : string.Empty;
        }
        else
        {
            ShowOrDeSelectButtons(GetOptionButtonsPlayer2);
            GetOptionButtonsPlayer2[index - 1].GetComponent<ButtonUtility>().SelectedStateAction();
            string[] name = GetOptionButtonsPlayer2[index - 1].name.Split('_');
            AnnouncerDesc.text = name.Length > 0 ? name[0] + " Selected" : string.Empty;
        }
    }

    public void SetParticleGameObject(string animationName,int ActorNumber)
    {
        if (ActorNumber == 1) playerOrb1.SetParticleGameObject(animationName);
        if (ActorNumber == 2) playerOrb2.SetParticleGameObject(animationName);
    }
    private void ResetOrbImages()
    {
        playerOrb1.SetParticleGameObject(GetOrbAnimationName[0]);
        playerOrb2.SetParticleGameObject(GetOrbAnimationName[0]);
        playerOrb1.ScaleUpOrb();
        playerOrb2.ScaleUpOrb();
    }
    private void ShowOrDeSelectButtons(Button[] buttons,bool show=true)
    {
        foreach (var item in buttons)
        {
            item.gameObject.SetActive(show);
            item.GetComponent<ButtonUtility>().DeSelectState();
        }
    }
    private void ClearUIForResult(bool show)
    {
        elementGameobjectPlayerA.SetActive(show);
        elementGameobjectPlayerB.SetActive(show);
        staticUIPanel.SetActive(show);
    }
    public void SetPlayerReady(int playerNum)
    {
        if(playerNum == 1)
            isPlayer1R = true;
        if(playerNum == 2)
            isPlayer2R = true;
    }

    string GenerateAnnouncerDescString(PlayTrun turn) => (turn.playerA, turn.playerB) switch
    {
        (1, 2) or (2, 1) => "<color=#83C878>Terra</color> beats <color=#5189BD>Torrent</color>",
        (2, 3) or (3, 2) => "<color=#5189BD>Torrent</color> beats <color=#EE8868>Blaze</color>",
        (3, 1) or (1, 3) => "<color=#EE8868>Blaze</color> beats <color=#83C878>Terra</color>",
        _ => "TIE",
    };

    string GenerateAnnouncerDescStringText(PlayTrun turn) => (turn.playerA, turn.playerB) switch
    {
        (1, 2) or (2, 1) => "TERRA BEATS TORRENT",
        (2, 3) or (3, 2) => "TORRENT BEATS BLAZE",
        (3, 1) or (1, 3) => "BLAZE BEATS TERRA",
        _ => "ITS A TIE",
    };

    private void SetCardWonAndLost(PlayTrun turn)
    {
       switch(turn.playerA,turn.playerB)
        {
            case (1, 2) or (2, 1):
                cardWon = CardType.TERRA;
                cardLost = CardType.TORRENT;
                vfxTag = AudioTag.TERRA;
                break;
            case (2, 3) or (3, 2):
                cardWon = CardType.TORRENT;
                cardLost = CardType.BLAZE;
                vfxTag = AudioTag.TORRENT;
                break;
            case (3, 1) or (1, 3):
                cardWon = CardType.BLAZE;
                cardLost = CardType.TERRA;
                vfxTag = AudioTag.BLAZE;
                break;
            default:
                cardWon = CardType.NONE;
                cardLost = CardType.NONE;
                vfxTag = AudioTag.None;
                break;
        }
    }

    void SetAnnouncerHeader(bool PlayerAWon,bool isDraw)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber == 1) // 1 is Player A
        {
            AnnouncerHeader.text = isDraw ? "ITS A TIE" : PlayerAWon ? "YOU WON" : "YOU LOSE";
            AnnouncerHeader.color = isDraw ? Color.white : PlayerAWon ? gameConfig.winColor : gameConfig.lostColor;

           AudioTag tag = isDraw ? AudioTag.Tie : PlayerAWon ? AudioTag.PointGain : AudioTag.PointLose;
            audioManager.PlayAudio(tag);

            AnnouncerHeader.GetComponent<TextFadeInEffect>().ShowEffect(AnnouncerHeader.color);
        }
        else if(PhotonNetwork.LocalPlayer.ActorNumber == 2) // not writing else just to make sure.....
        {
        
            AnnouncerHeader.text = isDraw ? "ITS A TIE" : !PlayerAWon ? "YOU WON" : "YOU LOSE";
            AnnouncerHeader.color = isDraw ? Color.white : !PlayerAWon ? gameConfig.winColor : gameConfig.lostColor;
            AudioTag tag = isDraw ? AudioTag.Tie : !PlayerAWon ? AudioTag.PointGain : AudioTag.PointLose;
            audioManager.PlayAudio(tag);
            AnnouncerHeader.GetComponent<TextFadeInEffect>().ShowEffect(AnnouncerHeader.color);
        }

    }

}
