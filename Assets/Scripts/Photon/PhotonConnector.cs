using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using PlayFab.ClientModels;
using PlayFab;

public class PhotonConnector : MonoBehaviourPunCallbacks
{
    public static PhotonConnector instance;


    [Header("Photon Debugs")]
    [SerializeField] private string nickName;

    public bool isRetryingMatch = false;

    private string nameKey = "displayName";
    public string roomNameKey = "roomName";

    ExitGames.Client.Photon.Hashtable _CustomRoomProprties = new ExitGames.Client.Photon.Hashtable();

    private bool isPlayerRejoining = false;

    private ApiManager apiManager;

    private EtherOrbManager etherOrbManager;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        GameObject[] objs = GameObject.FindGameObjectsWithTag("PhotonManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        apiManager = ApiManager.Instance;

        etherOrbManager = EtherOrbManager.Instance;
    }



    #region Custom Functions

    public void ConnectPhoton()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsConnected)
            return;

        ConnectToPhoton();

        nickName = PlayfabConnet.instance.PlayerName;
        MenuManager.instance.nameText.text = nickName;
    }

    public void MatchMakingTimerCompleted()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        { 
            PhotonNetwork.LoadLevel(1);
        }
    }
    void ConnectToPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }


    public void FindMatch()
    {
        //Try to join a pre existing room - if it fails, create one
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomOrCreateRoom();
            Debug.Log("Searching for A Game");
        }
        else
        {
            ConnectPhoton();
        }
    }

    void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);
        RoomOptions roomOptions =
            new RoomOptions()
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 2,
                CleanupCacheOnLeave = true
            };
        PhotonNetwork.CreateRoom("RoomName_" + randomRoomName, roomOptions);
        Debug.Log("Room Has been Created, Waiting for Another Player, Room Name: " + "RoomName_" + randomRoomName);
        _CustomRoomProprties.Add("StartTime", 0);
        _CustomRoomProprties.Add("RoundTime", 0);
        PhotonNetwork.LocalPlayer.CustomProperties = _CustomRoomProprties;
        MenuManager.instance.countdownTimer.gameObject.SetActive(true);
    }

    public void StopSearch()
    {
        if(PhotonNetwork.CurrentRoom !=null)
            PhotonNetwork.LeaveRoom();
        Debug.Log("Searching for match has been stoped ");
    }


    public void UsernameValueChange(TMP_InputField InputText)
    {
        PlayerPrefs.SetString(nameKey, InputText.text);

        PhotonNetwork.NickName = InputText.text + "_" + EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress();
        Debug.Log(PhotonNetwork.NickName);
    }

    public void DisconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    #endregion

    #region Pun CallBacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("You have connected to the Photon Master Server" + PhotonNetwork.IsConnected);
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        PhotonNetwork.NickName = nickName + "_" + EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress();
        Debug.Log(PhotonNetwork.NickName + " onConnet PhotonConnector-- NickName");
    }
    public override void OnJoinedLobby()
    {
        if (!isRetryingMatch)
        {
            Debug.Log("You have connected to a Photon Lobby");

            StartCoroutine(LoadMenu());
            if(PlayerPrefs.HasKey(roomNameKey))
            {
                Debug.Log("Room exists--->" + PlayerPrefs.GetString(roomNameKey));
                PhotonNetwork.JoinRoom(PlayerPrefs.GetString(roomNameKey));
            }
        }
        else
        {
            FindMatch();
        }
    }

    private IEnumerator LoadMenu()
    {
        if (!string.IsNullOrEmpty(EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress()))
        {
            yield return new WaitUntil(() => EtherOrbManager.Instance.isUserDataReady);

            Debug.Log("LoadMenu NFTs cound--->" + EtherOrbManager.Instance.userModel.data.nfts.Count);

            while (EtherOrbManager.Instance.userModel.data.nfts.Count != EtherOrbManager.Instance.nftMetaData.OrbDetails.Count)
            {
                Debug.Log("LoadMenu.userModel---> " + EtherOrbManager.Instance.userModel.success);
                yield return new WaitForSeconds(1f);
            }
        }
        Debug.Log("nftMetaDatas---->" + EtherOrbManager.Instance.nftMetaData.OrbDetails.Count);
        OrbManager.instance.GetAllOrbDetails();
        MenuManager.instance.OpenMenuId(2);
        MenuManager.instance.screenSwipe.RefreshContents();
        yield return null;
        OrbManager.instance.SelectFirstObject();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log(cause);
        if (isRetryingMatch)
        {
            ConnectPhoton();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Could not find room ---- Creating a Room,  Message: " + message);
        MakeRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Could not Create Room ----- Retrying to create Room,  Message: " + message);
        MakeRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        PlayerPrefs.DeleteKey(roomNameKey);
        Debug.Log("match already finished--->" + message);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            //MenuManager.instance.hintText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/2 Starting Game";
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            Debug.Log("Player left the room--->");
        }
        else
        {
            Debug.Log("Searching for A Game");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //TimerScript.instance.TimeRpc();
            MenuManager.instance.countdownTimer.StartTimer();
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if(GameManager.instance!=null)
            {
                Debug.Log("Rejoin Player --->" + newPlayer.NickName);
                //TODO :- Need to change the logic for the ReConnect...
                //GameManager.instance.ReJoinPlayer(newPlayer);
            }
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount + "/2 Starting Game");
        }
    }

    #endregion
}
