using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
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

    private int numberOfBothPlayerFailedAttempt = 0;
    public int numberOfPlayerAFailedAttempt = 0;
    public int numberOfPlayerBFailedAttempt = 0;

    public List<string> userWallets = new List<string>();

    ExitGames.Client.Photon.Hashtable _CustomRoomProprties = new ExitGames.Client.Photon.Hashtable();

    private string currentRoundKey = "'currentRound";

    public bool isPlayerRejoining = false;

    private UserMatchHistory userMatchHistory = new UserMatchHistory();

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        rTimer = GetComponent<RoundTimer>();
        players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log(players[i].NickName + " Actor name, " + players[i].ActorNumber + " Actor number");
            string [] userString = players[i].NickName.Split("_");
            UNames[i].text = userString[0];

            if(userString.Length>0)
                userWallets.Add(userString[1]);
        }
        roundNumText.text = "Round " + roundNum;
        UScores[0].text = "0";
        UScores[1].text = "0";
        gameState = GameState.Nothing;

        homeButton.onClick.AddListener(LeaveGame);
        reMatchButton.onClick.AddListener(SendRematchRequest);

        _CustomRoomProprties.Clear();
    }
    private void Start()
    {
        warningPanel = EtherOrbManager.Instance.WarningPanel;
        audioManager = EtherOrbManager.Instance.AudioManager;

        if (PlayerPrefs.HasKey(PhotonConnector.instance.roomNameKey))
        {
            Debug.Log("Loading game scene Room exists--->" + PlayerPrefs.GetString(PhotonConnector.instance.roomNameKey));
            int currentRound = (int)PhotonNetwork.CurrentRoom.CustomProperties[currentRoundKey];
            Debug.Log("current Round ---> " + currentRound);

            for(int i=1; i<=currentRound;i++)
            {
                Debug.Log((string)PhotonNetwork.CurrentRoom.CustomProperties["Round" + i]);
            }

            isPlayer1R = isPlayer2R =true;
        }

        //TODO :- Need to change the logic for the ReConnect...
        //PlayerPrefs.SetString(PhotonConnector.instance.roomNameKey, PhotonNetwork.CurrentRoom.Name);
        //PlayfabConnet.instance.UpdateLastMatchId(PhotonNetwork.CurrentRoom.Name);
        //PlayerPrefs.Save();
    }

    public void ReJoinPlayer(Player player)
    {
        GameObject playerManager = PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity);
        isPlayerRejoining = true;
        if (playerManager1 ==null)
        {
            Debug.Log("ReJoinPlayer playerManager1---->");
            playerManager1 = playerManager.GetComponent<PlayerManager>();
            playerManager1.SetActorNum(1,player);
        }
        else if(playerManager2==null)
        {
            Debug.Log("ReJoinPlayer playerManager2---->");
            playerManager2 = playerManager.GetComponent<PlayerManager>();
            playerManager2.SetActorNum(2,player);
        }
    }

    private void ResetUIForRematch()
    {
        roundNum = 0;
        roundNumText.text = "Round " + roundNum;
        UScores[0].text = "0";
        UScores[1].text = "0";
        gameState = GameState.Nothing;
        GameOverPanel.SetActive(false);
        DefeatPanel.SetActive(false);
        VictoryPanel.SetActive(false);
        playerAScore = playerBScore = 0;
        warningPanel.HideWarning();
        warningPanel.HidePopUp();
        numberOfBothPlayerFailedAttempt = 0;
        numberOfPlayerAFailedAttempt = 0;
        numberOfPlayerBFailedAttempt = 0;
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
       // Implement if needed...
    }

    public void SaveAndPlayChoices(int choice, int ActorNumber)
    {
        SaveChoices(choice, ActorNumber);
        PlayChoices();

        if (currentPlay.playerA != 0 && currentPlay.playerB != 0)
        {
            gameState = GameState.RoundEnd;
            audioManager.StopTimerSound();
            rTimer.StopTimer();
        }
    }

    public void PlayChoices()
    {
        if (currentPlay.playerA != 0 && currentPlay.playerB != 0)
        {
            RevealChoices();
        }
    }

    public void RevealChoices()
    {
        //Debug.Log("RevealChoices---->");
        SetParticleGameObject(GetOrbAnimationName[currentPlay.playerA], 1);
        SetParticleGameObject(GetOrbAnimationName[currentPlay.playerB], 2);
    }

    public void SaveChoices(int choice, int ActorNumber)
    {
        //Debug.Log("SaveAndPlayChoices---->" + choice + "--->ActorNumber " + ActorNumber);
        if (ActorNumber %2 == 1) currentPlay.playerA = choice;
        if (ActorNumber %2 == 0) currentPlay.playerB = choice;
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
        if(currentPlay.playerA == 0 && currentPlay.playerB == 0)
        {
            //Lets draw the match as 2 failed attempts from both player...
            isDraw = true;
            numberOfBothPlayerFailedAttempt++;
        }
        else if (currentPlay.playerA == 0 || currentPlay.playerB == 0)
        {
            //Lets consider a when 1st player failed to attempt the option...
            if (currentPlay.playerA == 0)
            {
                numberOfPlayerAFailedAttempt++;
                if (numberOfPlayerAFailedAttempt < 2)
                {
                    currentPlay.playerA = RandomPlay.playerA; // 1-3
                    isDraw = true;
                    PlayerAWon = false;
                }
            }
            //Lets consider a when 2nd player failed to attempt the option...
            else if (currentPlay.playerB == 0)
            {
                numberOfPlayerBFailedAttempt++;
                if (numberOfPlayerBFailedAttempt < 2)
                {
                    // give point to playerA...
                    currentPlay.playerB = RandomPlay.playerB; // 1-3
                    isDraw = true;
                    PlayerAWon = false;
                }
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
            if (numberOfPlayerAFailedAttempt >= 2)
            {
                playerBScore += 1;
                PlayerAWon = false;
            }
            else if (numberOfPlayerBFailedAttempt >= 2)
            {
                playerAScore += 1;
                PlayerAWon = true;
            }
            else
            {
                if (currentPlay.playerA != 0 && currentPlay.playerB != 0)
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
        string roundKey = "Round" + roundNum;
        if (PhotonNetwork.LocalPlayer.ActorNumber  %2 == 1)
        {
            if (PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorA, impactAnimatorB,playerOrb2));

                if (!_CustomRoomProprties.ContainsKey(roundKey))
                    _CustomRoomProprties.Add(roundKey, PhotonNetwork.NickName);

                if (!_CustomRoomProprties.ContainsKey(currentRoundKey))
                    _CustomRoomProprties.Add(currentRoundKey, roundNum);

                _CustomRoomProprties[currentRoundKey] = roundNum;

                Debug.Log("_CustomRoomProprties ----> " + _CustomRoomProprties[currentRoundKey]);

                PhotonNetwork.CurrentRoom.SetCustomProperties(_CustomRoomProprties);
            }
            else if(!PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorB, impactAnimatorA,playerOrb1));
            }
            else if(isDraw)
            {
                if (!_CustomRoomProprties.ContainsKey(roundKey))
                    _CustomRoomProprties.Add(roundKey, "draw");

                if (!_CustomRoomProprties.ContainsKey(currentRoundKey))
                    _CustomRoomProprties.Add(currentRoundKey, roundNum);

                _CustomRoomProprties[currentRoundKey] = roundNum;
                Debug.Log("_CustomRoomProprties----> " + _CustomRoomProprties[currentRoundKey]);
                PhotonNetwork.CurrentRoom.SetCustomProperties(_CustomRoomProprties);
            }
        }

        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 0)
        {
            if (PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorA, impactAnimatorB,playerOrb2));
            }
            else if (!PlayerAWon && !isDraw)
            {
                yield return StartCoroutine(ExecuteAnimationSequence(zapAnimatorB, impactAnimatorA,playerOrb1));
                _CustomRoomProprties.Add(roundKey, PhotonNetwork.NickName);
                if (!_CustomRoomProprties.ContainsKey(currentRoundKey))
                    _CustomRoomProprties.Add(currentRoundKey, roundNum);

                _CustomRoomProprties[currentRoundKey] = roundNum;

                Debug.Log("_CustomRoomProprties----> " + _CustomRoomProprties[currentRoundKey]);
                PhotonNetwork.CurrentRoom.SetCustomProperties(_CustomRoomProprties);
            }
        }

        spawnlastPlayUi(PlayerAWon, isDraw);
        SetAnnouncerHeader(PlayerAWon, isDraw);
        AnnouncerDesc.GetComponent<TextRevealEffect>().StartEffect(GenerateAnnouncerDescStringText(currentPlay));
    }

    private IEnumerator ExecuteAnimationSequence(Animator zap,Animator imapct, OrbGameUI orbGameUI)
    {
        WinVFXNames winVFX = gameConfig.winVFXNames.Find(x => x.type.Equals(cardWon));
        audioManager.PlayAudio(vfxTag);

        if (winVFX != null)
        {
            zap.gameObject.SetActive(true);
            zap.enabled = true;
            zap.Play(winVFX.zapAnimationName);
            yield return new WaitForSeconds(0.517f);
            zap.enabled = false;
            zap.gameObject.SetActive(false);
        }

        LostAnimationName lostVFX = gameConfig.lostAnimationNames.Find(x => x.type.Equals(cardLost));
        imapct.gameObject.SetActive(true);
        imapct.enabled = true;
        if(winVFX !=null)
            imapct.Play(winVFX.impactAnimationName);
        if(lostVFX!=null)
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
        
        if (numberOfBothPlayerFailedAttempt>=2)
        {
            gameState = GameState.GameEnd;
        }
        else if(playerAScore < 3 && playerBScore < 3)
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
        reMatchButton.interactable = true;
        GameOverPanel.SetActive(true);
        PlayerPrefs.DeleteKey(PhotonConnector.instance.roomNameKey);
        isPlayerRejoining = false;
        CalculateWinLoss();
        PlayfabConnet.instance.UpdateGamesPlayed();
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

        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 1)
        {
            EndPanelScore.text = GetPlayerScore(1).ToString("0");
            VictoryPanel.SetActive(playerAWon);

            AudioTag tag = playerAWon ? AudioTag.WinScreen : AudioTag.LoseScreen;
            audioManager.PlayAudio(tag);

            DefeatPanel.SetActive(!playerAWon);
            ExecuteMatchComplete(playerAWon ? userWallets[0] : userWallets[1]);
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 0)
        {
            EndPanelScore.text = GetPlayerScore(2).ToString("0");
            VictoryPanel.SetActive(!playerAWon);

            AudioTag tag = !playerAWon ? AudioTag.WinScreen : AudioTag.LoseScreen;
            audioManager.PlayAudio(tag);

            DefeatPanel.SetActive(playerAWon);
            ExecuteMatchComplete(playerAWon ? userWallets[0] : userWallets[1]);
        }
    }

    private void ExecuteMatchComplete(string winningAddress)
    {
        //if (!string.IsNullOrEmpty(EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress()))
        //{
        //    Debug.Log("winnerAddress----> " + winningAddress);
        //    ApiManager.Instance.CompleteMatchWithNFT(PhotonNetwork.CurrentRoom.Name, winningAddress);
        //}
        EtherOrbManager.Instance.CompleteMatchWithNFT(PhotonNetwork.CurrentRoom.Name, winningAddress);
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
        if (PhotonNetwork.LocalPlayer.ActorNumber %2== 1) // 1 is Player A
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
        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 0) 
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
        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 1) // 1 is Player A
        {
            if(playernum %2 ==1)
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
        else if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 0) // 1 is Player A
        {
            if(playernum %2 ==0)
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
        pv.RPC(nameof(RPC_SendMatchRematchAccept), RpcTarget.All);
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
        pv.RPC(nameof(RPC_LeaveRoom), RpcTarget.OthersBuffered);
        StartCoroutine(LoadMenu());
    }

    [PunRPC]
    void RPC_LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        reMatchButton.interactable = false;
        Debug.Log("Player left the game...");
        EtherOrbManager.Instance.WarningPanel.ShowWarning("Player left the game...");
    }

    private IEnumerator LoadMenu()
    {
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        Debug.Log("Leaving Room --->");
        PhotonNetwork.LoadLevel(0);
    }
    public void GameFailedExit()
    {
        PhotonNetwork.LeaveRoom();
        reMatchButton.interactable = false;
        Debug.Log("Game has ended Because a player left the room...");
        EtherOrbManager.Instance.WarningPanel.ShowWarning("Player left the game...");
        if (gameCompleted)
            return;
        gameState = GameState.Nothing;
        rTimer.StopTimer();
        audioManager.StopTimerSound();
        GameOverPanel.SetActive(true);
        EndPanelHeader.text = "Game has ended Because a player left the game in middile...";
        EndPanelScore.gameObject.SetActive(false);
    }

    public int GetPlayerScore(int player)
    {
        if (player%2 == 1)
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
        if (PhotonNetwork.LocalPlayer.ActorNumber%2 == 1)
        {
            PlayfabConnet.instance.addtoPlayerTotalScore(player1);
            SetAndSendResultData(playerAScore, playerBScore);
        }
        if (PhotonNetwork.LocalPlayer.ActorNumber %2== 0)
        {
            PlayfabConnet.instance.addtoPlayerTotalScore(player2);
            SetAndSendResultData(playerBScore, playerAScore);
        }
    }

    private void SetAndSendResultData(int score1, int score2)
    {
        string matchScore = score1 + "-" + score2;
        int matchWon = (score1 == score2)  ? (int)MatchStatus.draw : score1 > score2 ? (int)MatchStatus.win : (int)MatchStatus.loss;
        float reward = score1 > score2 ? +1f : -1f;
        SendMatchHistory(matchScore, matchWon, reward);
    }

    private void SendMatchHistory(string matchScore,int matchWon,float reward)
    {
        userMatchHistory.matchId = PhotonNetwork.CurrentRoom.Name;
        userMatchHistory.matchWon = matchWon;
        userMatchHistory.matchScore = matchScore;
        userMatchHistory.wageredAmount = 1;
        userMatchHistory.reward = reward;
        userMatchHistory.orbId = string.Empty; 
        userMatchHistory.orbImageUrl = string.Empty;
        PlayfabConnet.instance.FetchServerTime(OnServerTimeSuccess, OnServerTimeFailure);
    }

    private void OnServerTimeSuccess(GetTimeResult result)
    {
        // Server timestamp retrieved successfully
        long unixTimestamp = (long)result.Time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        Debug.Log("Unix timestamp: " + unixTimestamp);
        userMatchHistory.timestamp = unixTimestamp;

        //PlayfabConnet.instance.UpdatePlayerHistory(userMatchHistory);
    }

    private void OnServerTimeFailure(PlayFabError error)
    {
        // Failed to retrieve server timestamp
        Debug.LogError("Error fetching server time: " + error.ErrorMessage);

        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        userMatchHistory.timestamp = unixTime;

        //PlayfabConnet.instance.UpdatePlayerHistory(userMatchHistory);
    }

    void GenerateRandomOption()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;
        if (currentPlay.playerA == 0 && currentPlay.playerB == 0)
        {
            //when both player have not played...
            pv.RPC(nameof(RPC_StoreRandomOption), RpcTarget.All, 0, 0);
            Debug.Log("No one played input");
            AnnouncerDesc.text = "NO Input Selected";
        }
        else if(currentPlay.playerA == 0)
        {
            Debug.Log("Random Generated: A-");
            pv.RPC(nameof(RPC_StoreRandomOption), RpcTarget.All, 0, currentPlay.playerB);
            AnnouncerDesc.text = "NO Input Selected";
        }
        else if(currentPlay.playerB == 0)
        {
            int RandomB = numberOfPlayerAFailedAttempt < 1 ? UnityEngine.Random.Range(1, 4):0;
            Debug.Log("Random Generated: B-");
            pv.RPC(nameof(RPC_StoreRandomOption), RpcTarget.All, currentPlay.playerA, 0);
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
            Debug.Log("SyncPlayerData---->");
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
        if(player%2 ==1)
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
        if (ActorNumber%2 == 1)
            playerOrb1.SetParticleGameObject(animationName);
        else 
            playerOrb2.SetParticleGameObject(animationName);
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

        if(PhotonNetwork.LocalPlayer.ActorNumber %2==1)
        {
            //player 1 --- Hiding Ready button for player2...
            GetOptionButtonsPlayer2[3].gameObject.SetActive(false);
        }
        else
        {
            //player 2 --- Hiding Ready button for player1...
            GetOptionButtonsPlayer1[3].gameObject.SetActive(false);
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
        if(playerNum %2 == 1)
            isPlayer1R = true;
        else
            isPlayer2R = true;

        Debug.Log("SetPlayerReady--->" + isPlayer1R + "  ---> isPlayer2R " + isPlayer2R);
    }

    private string GenerateAnnouncerDescString(PlayTrun turn) => (turn.playerA, turn.playerB) switch
    {
        (1, 2) or (2, 1) => "<color=#83C878>Terra</color> beats <color=#5189BD>Torrent</color>",
        (2, 3) or (3, 2) => "<color=#5189BD>Torrent</color> beats <color=#EE8868>Blaze</color>",
        (3, 1) or (1, 3) => "<color=#EE8868>Blaze</color> beats <color=#83C878>Terra</color>",
        _ => "TIE",
    };

    private string GenerateAnnouncerDescStringText(PlayTrun turn) => (turn.playerA, turn.playerB) switch
    {
        (1, 2) or (2, 1) => "TERRA BEATS TORRENT",
        (2, 3) or (3, 2) => "TORRENT BEATS BLAZE",
        (3, 1) or (1, 3) => "BLAZE BEATS TERRA",
        (0, 1) or (1, 0) => "TERRA WON",
        (0, 2) or (2, 0) => "TORRENT WON",
        (0, 3) or (3, 0) => "BLAZE WON",
        (1,1) => playerManager1.TerraValue != playerManager2.TerraValue ? "TERRA WON" : "ITS A TIE",
        (2, 2) => playerManager1.TerraValue != playerManager2.TerraValue ? "TORRENT WON" : "ITS A TIE",
        (3, 3) => playerManager1.TerraValue != playerManager2.TerraValue ? "BLAZE WON" : "ITS A TIE",
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
            case (0, 1) or (1, 0):
                cardWon = CardType.TERRA;
                cardLost = CardType.NONE;
                vfxTag = AudioTag.TERRA;
                break;
            case (0, 2) or (2, 0):
                cardWon = CardType.TORRENT;
                cardLost = CardType.NONE;
                vfxTag = AudioTag.TORRENT;
                break;
            case (0, 3) or (3, 0):
                cardWon = CardType.BLAZE;
                cardLost = CardType.NONE;
                vfxTag = AudioTag.BLAZE;
                break;
            case (1, 1):
                cardWon = CardType.TERRA;
                cardLost = CardType.TERRA;
                vfxTag = AudioTag.TERRA;
                break;
            case (2, 2):
                cardWon = CardType.TORRENT;
                cardLost = CardType.TORRENT;
                vfxTag = AudioTag.TORRENT;
                break;
            case (3, 3):
                cardWon = CardType.BLAZE;
                cardLost = CardType.BLAZE;
                vfxTag = AudioTag.BLAZE;
                break;
            default:
                cardWon = CardType.NONE;
                cardLost = CardType.NONE;
                vfxTag = AudioTag.None;
                break;
        }
    }
    private void SetAnnouncerHeader(bool PlayerAWon,bool isDraw)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber%2 == 1) // 1 is Player A
        {
            AnnouncerHeader.text = isDraw ? "ITS A TIE" : PlayerAWon ? "YOU WON" : "YOU LOSE";
            AnnouncerHeader.color = isDraw ? Color.white : PlayerAWon ? gameConfig.winColor : gameConfig.lostColor;

           AudioTag tag = isDraw ? AudioTag.Tie : PlayerAWon ? AudioTag.PointGain : AudioTag.PointLose;
            audioManager.PlayAudio(tag);

            AnnouncerHeader.GetComponent<TextFadeInEffect>().ShowEffect(AnnouncerHeader.color);
        }
        else if(PhotonNetwork.LocalPlayer.ActorNumber%2 == 0) // not writing else just to make sure.....
        {
            AnnouncerHeader.text = isDraw ? "ITS A TIE" : !PlayerAWon ? "YOU WON" : "YOU LOSE";
            AnnouncerHeader.color = isDraw ? Color.white : !PlayerAWon ? gameConfig.winColor : gameConfig.lostColor;
            AudioTag tag = isDraw ? AudioTag.Tie : !PlayerAWon ? AudioTag.PointGain : AudioTag.PointLose;
            audioManager.PlayAudio(tag);
            AnnouncerHeader.GetComponent<TextFadeInEffect>().ShowEffect(AnnouncerHeader.color);
        }

    }

}
