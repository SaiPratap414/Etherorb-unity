using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EtherOrbManager : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private WarningPanel warningPanel;

    public AudioManager AudioManager { get { return audioManager; } }
    public WarningPanel WarningPanel { get { return warningPanel; } }

    private static EtherOrbManager instance;
    public static EtherOrbManager Instance { get { return instance; } }

    public UserModel userModel { get; private set; }

    public NFTMeta nftMetaData = new NFTMeta();

    public bool isUserDataReady { get; private set; }

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

    public void GetUserNFTs()
    {
        isUserDataReady = false;
        if (!string.IsNullOrEmpty(WarningPanel.GetUserWalletAddress()))
        {
            string endPoint = $"user/{WarningPanel.GetUserWalletAddress()}";
            ApiManager.Instance.GetRequestFromURL(endPoint, OnGetUserNFTSuccess, OnGetUserNFTError);
        }
    }
    public void StartMatchWithNFT(string roomId)
    {
        if (!string.IsNullOrEmpty(WarningPanel.GetUserWalletAddress()))
        {
            string endPoint = $"match/{roomId}/start";
            MatchData matchData = new MatchData(WarningPanel.GetUserWalletAddress(), 2, userModel.data.nfts[0]);
            string postData = JsonUtility.ToJson(matchData);
            ApiManager.Instance.PostRequestFromURL(endPoint, postData, OnNFTStartMatchSuccess, OnNFTStartMatchFailed);
        }
    }

    public void CompleteMatchWithNFT(string roomName, string winningAddress)
    {
        string endPoint =  $"match/{roomName}/complete";
        if (!string.IsNullOrEmpty(WarningPanel.GetUserWalletAddress()))
        {
            MatchCompleteModel matchCompleteModel = new MatchCompleteModel(winningAddress);
            string postData = JsonUtility.ToJson(matchCompleteModel);
            ApiManager.Instance.PostRequestFromURL(endPoint, postData, OnNFTCompleteMatchSuccess, OnNFTCompleteMatchFailed);

        }
    }

    private void OnGetUserNFTSuccess(string response)
    {
        isUserDataReady = true;
        userModel = JsonUtility.FromJson<UserModel>(response);
        MenuManager.instance.NFTScreen();
        foreach (var item in userModel.data.nfts)
        {
            //TODO :- Fetch NFT data from here...
            //GetAndSetNFTMetaData(item);
            string endPoint = $"metadata/{item}";
            ApiManager.Instance.GetRequestFromURL(endPoint, OnNFTMetaDataSuccess, OnNFTMetaDataFailed);
        }
    }
    private void OnGetUserNFTError(string errorCause)
    {
        isUserDataReady = false;
        WarningPanel.ShowWarning("Error occurred. Please try again...");
    }

    private void OnNFTMetaDataSuccess(string response)
    {
        NFTMetaData metaData = JsonUtility.FromJson<NFTMetaData>(response);
        nftMetaData.OrbDetails.Add(metaData);
    }
    private void OnNFTMetaDataFailed(string errorCause)
    {
        WarningPanel.ShowWarning("Error occurred. Please try again...");
    }
    private void OnNFTStartMatchSuccess(string response)
    {
        Debug.Log("OnNFTStartMatchSuccess ---> " + response);
    }
    private void OnNFTStartMatchFailed(string errorCause)
    {
        WarningPanel.ShowWarning("Could not start the match.Please try again.");
        MenuManager.instance.StopFindMatch();
    }
    private void OnNFTCompleteMatchSuccess(string response)
    {
        Debug.Log("CompleteMatchWithNFT ---> " + response);
    }
    private void OnNFTCompleteMatchFailed(string errorCause)
    {
        Debug.Log("CompleteMatchWithNFT ---> " + errorCause);
    }
}
