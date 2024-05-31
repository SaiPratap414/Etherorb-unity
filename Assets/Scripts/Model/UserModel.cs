using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UserModel : UserBase
{
    public string address;
    public UserData data;
}

[Serializable]
public class UserBase
{
    public bool success;
}

[Serializable]
public class UserData
{
    public string address;
    public List<string> nfts;
}

[Serializable]
public class NFTData
{
    public string address;
    public int amount;

    public NFTData(string addr,int amt)
    {
        address = addr;
        amount = amt;
    }
}

[Serializable]
public class MatchData : NFTData
{
    public string nftid;

    public MatchData(string addr,int amt,string _nftId) : base(addr,amt)
    {
        address = addr;
        amount = amt;
        nftid = _nftId;
    }
}

[Serializable]
public class StartMatchModel
{
    public bool success;
    public UserBase data;
}

[Serializable]
public class MatchCompleteModel
{
    public string winner;

    public MatchCompleteModel(string wnr)
    {
        winner = wnr;
    }
}

[Serializable]
public class UserMatchHistory
{
    public string matchId;
    public int matchWon;
    public float wageredAmount;
    public float reward;
    public string matchScore;
    public long timestamp;

    public string GetMatchStatus()
    {
        switch(matchWon)
        {
            case (int)MatchStatus.draw:
                return "Draw";
            case (int)MatchStatus.win:
                return "Win";
            case (int)MatchStatus.loss:
                return "Loss";
            default:
                return string.Empty;

        }
    }

    public Color GetMatchColor()
    {
        switch (matchWon)
        {
            case (int)MatchStatus.draw:
                return new Color(198f/255f, 200f/255f, 120f/255f,1f);
            case (int)MatchStatus.win:
                return new Color(73f/255f, 179f/255f, 78f/255f, 1f);
            case (int)MatchStatus.loss:
                return new Color(161f/255f, 68f/255f, 68f/255f,1f);
            default:
                return Color.white;

        }
    }

    public string GetDate()
    {
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp).ToLocalTime();
        return dt.ToString("dd/MM").Replace("-","/");
    }
    public string GetTime()
    {
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp).ToLocalTime();
        return dt.ToString("HH/mm").Replace("-", ":");
    }
}

public enum MatchStatus
{
    draw =0,
    win =1,
    loss=2
}

[Serializable]
public class MatchHistory
{
    public List<UserMatchHistory> userMatchHistories = new List<UserMatchHistory>();

    public int totalGamesPlayed {get{ return userMatchHistories.Count; }}

    public int totalWinMatches { get { return userMatchHistories.FindAll(x => x.matchWon.Equals((int)MatchStatus.win)).Count; } }

    public int totatLossMatches { get { return userMatchHistories.FindAll(x => x.matchWon.Equals((int)MatchStatus.loss)).Count; } }

    public int totatDrawMatches { get { return userMatchHistories.FindAll(x => x.matchWon.Equals((int)MatchStatus.draw)).Count; } }

    public float winRate { get { return (float)totalWinMatches / (float)totalGamesPlayed; } }
}

[Serializable]
public class NFTAttributes
{
    public int Blaze;
    public int Torrent;
    public int Terra;
    public double Aer;
    public int Ether;
    public string type;

    public NFTAttributes()
    {

    }
}

[Serializable]
public class NFTMetaData
{
    public string name;
    public string description;
    public string image_url;
    public string id;
    public string animation_url;
    public NFTAttributes attributes;

    public NFTMetaData(string id, string image, int terra, int torrent, int blaze)
    {
        this.id = id;
        image_url = image;
        attributes.Terra = terra;
        attributes.Torrent = torrent;
        attributes.Blaze = blaze;
    }
}

[Serializable]
public class NFTMeta
{
    public List<NFTMetaData> OrbDetails = new List<NFTMetaData>();
}



