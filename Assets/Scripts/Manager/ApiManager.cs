using System;
using CI.HttpClient;
using UnityEngine;
using Photon.Pun;

public class ApiManager
{

    private static ApiManager instance;

    private string baseURL = "https://helpless-octopus-49.telebit.io/";

    public UserModel userModel { get; private set; }

    private string userWalletAdress;

    public bool matchStartingKey { get; private set; }

    public static ApiManager Instance
    {
        get
        {
            if (instance == null)
                instance = new ApiManager();
            return instance;
        }
    }

    public void GetUserNFTs(string address)
    {
        userWalletAdress = address;
        HttpClient client = new HttpClient();
        string url = baseURL + $"user/{address}";
        client.Get(new Uri(url), HttpCompletionOption.AllResponseContent, (r) =>
        {
            userModel = JsonUtility.FromJson<UserModel>(r.ReadAsString());
            if (userModel.success)
            {
                if (userModel.data.nfts ==null || userModel.data.nfts.Count==0)
                {
                    MenuManager.instance.NoNFTScreen();
                }
                else
                {
                    MenuManager.instance.NFTScreen();
                }
            }
            else
            {
                EtherOrbManager.Instance.WarningPanel.ShowWarning("Error occurred. Please try again...");
            }
        });
    }

    public void AddNFTs()
    {
        NFTData nftData = new NFTData(userWalletAdress, 2);
        HttpClient client = new HttpClient();
        StringContent content = StringContent.FromObject(nftData);
        string url = baseURL + $"airdrop";
        Debug.Log("NFT data ---> " + JsonUtility.ToJson(nftData) + " URL ---> " + url);
        client.Post(new Uri(url), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
            UserBase userBase = JsonUtility.FromJson<UserBase>(r.ReadAsString());
            if(userBase.success)
            {
                MenuManager.instance.NFTScreen();
            }
            else
            {
                MenuManager.instance.NoNFTScreen();
            }
        });
    }

    public void StartMatchWithNFT(string roomId)
    {
        MatchData matchData = new MatchData(userWalletAdress, 2, userModel.data.nfts[0]);
        HttpClient client = new HttpClient();
        StringContent content = StringContent.FromObject(matchData);
        string url = baseURL + $"match/{roomId}/start";
        Debug.Log("match data ---> " + JsonUtility.ToJson(matchData) + " URL ---> " + url);
        client.Post(new Uri(url), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
            Debug.Log(r.ReadAsString());
            StartMatchModel startMatchModel = JsonUtility.FromJson<StartMatchModel>(r.ReadAsString());
            if (startMatchModel.success)
            {
                // wait other player success msg....
            }
            else
            {
                EtherOrbManager.Instance.WarningPanel.ShowWarning("Could not start the match.Please try again.");
                MenuManager.instance.StopFindMatch();
            }
        });
    }

    public void CompleteMatchWithNFT(string roomId,string winner)
    {
        MatchCompleteModel matchCompleteModel = new MatchCompleteModel(winner);
        HttpClient client = new HttpClient();
        StringContent content = StringContent.FromObject(matchCompleteModel);
        string url = baseURL + $"match/{roomId}/complete";
        Debug.Log("match data ---> " + JsonUtility.ToJson(matchCompleteModel) + " URL ---> " + url);
        client.Post(new Uri(url), content, HttpCompletionOption.AllResponseContent, (r) =>
        {
            Debug.Log("CompleteMatchWithNFT ---> " + r.ReadAsString());
        });
    }
}

