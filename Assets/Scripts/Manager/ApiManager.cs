using System;
using CI.HttpClient;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;

public class ApiManager
{

    private static ApiManager instance;

    private string baseURL = "https://helpless-octopus-49.telebit.io/";

    public UserModel userModel { get; private set; }

    public NFTMeta nftMetaData = new NFTMeta();

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
        Debug.Log("GetUserNFTs--->" + url);
        client.Get(new Uri(url), HttpCompletionOption.AllResponseContent, (r) =>
        {
            userModel = JsonUtility.FromJson<UserModel>(r.ReadAsString());
            Debug.Log("userModel.success  --->" + userModel.success);
            if (userModel.success)
            {
                if (userModel.data.nfts ==null || userModel.data.nfts.Count==0)
                {
                    MenuManager.instance.NoNFTScreen();
                }
                else
                {
                    MenuManager.instance.NFTScreen();
                    Debug.Log("Nft count --->" + userModel.data.nfts.Count);
                    foreach (var item in userModel.data.nfts)
                    {
                        GetAndSetNFTMetaData(item);
                    }
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

    public void GetAndSetNFTMetaData(string nftId)
    {
        HttpClient client = new HttpClient();
        string url = baseURL + $"metadata/{nftId}";
        client.Get(new Uri(url), HttpCompletionOption.AllResponseContent, (r) =>
        {
            NFTMetaData metaData = JsonUtility.FromJson<NFTMetaData>(r.ReadAsString());
            nftMetaData.OrbDetails.Add(metaData);

        });
    }

    public void DownloadImage(string url,string savePath,Action<byte[]> onDownloadComplete)
    {
        HttpClient client = new HttpClient();
        client.Get(new Uri(url), HttpCompletionOption.AllResponseContent, (r) =>
        {
            //TODO : Add save logic when you have id for meta data...
            //saveImage(savePath, r.ReadAsByteArray());
            //byte []imageData = LoadImage(savePath);
            //GetSpite()
            onDownloadComplete(r.ReadAsByteArray());
        });
    }

    private void saveImage(string path, byte[] imageBytes)
    {
        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        try
        {
            File.WriteAllBytes(path, imageBytes);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    public byte[] LoadImage(string path)
    {
        byte[] dataByte = null;

        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Debug.LogWarning("Directory does not exist");
            return null;
        }

        if (!File.Exists(path))
        {
            Debug.Log("File does not exist");
            return null;
        }
        try
        {
            dataByte = File.ReadAllBytes(path);
            Debug.Log("Loaded Data from: " + path.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Load Data from: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
        return dataByte;
    }

    public Sprite GetSpite(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Debug.Log("File does not exist");
            return null;
        }
        else
        {
            Texture2D texture2D = new Texture2D(2,2);
            texture2D.LoadImage(LoadImage(fileName));
            return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
        }
    }

}

