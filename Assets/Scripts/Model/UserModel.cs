using System.Collections.Generic;
using UnityEngine;
using System;
using PooledScrollList.Data;

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
    public string orbImageUrl;
    public string orbId;
    public string aerValue;
    public int XP;
    public string walletID;
    public string username;

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
        return dt.ToString("HH:mm");
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

    public float winRate { get { return (float)totalWinMatches *100 / (float)totalGamesPlayed; } }

    public int XP { get { return ((int)winRate * 1000) + ((int)totalGamesPlayed * 10); } }
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
    public NFTAttributes attributes = new NFTAttributes();

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

[Serializable]
public class Leaderboard
{
    public string DisplayName;
    public string PlayFabId;
    public int Position;
    public Profile Profile;
    public int StatValue;
}
[Serializable]
public class Profile
{
    public object AdCampaignAttributions;
    public object AvatarUrl;
    public object BannedUntil;
    public object ContactEmailAddresses;
    public object Created;
    public string DisplayName;
    public object ExperimentVariants;
    public object LastLogin;
    public object LinkedAccounts;
    public object Locations;
    public object Memberships;
    public object Origination;
    public string PlayerId;
    public string PublisherId;
    public object PushNotificationRegistrations;
    public object Statistics;
    public object Tags;
    public string TitleId;
    public object TotalValueToDateInUSD;
    public object ValuesToDate;
}

[Serializable]
public class StatisticModel
{
    public object CustomTags;
    public int MaxResultsCount;
    public object ProfileConstraints;
    public int StartPosition;
    public string StatisticName;
    public object Version;
    public object AuthenticationContext;
}
[Serializable]
public class LeaderboardModel
{
    public List<Leaderboard> Leaderboard;
    public object NextReset;
    public int Version;
    public StatisticModel Request;
    public object CustomData;
}

[Serializable]
public class GameLeaderboardData
{
    public List<RankLeaderBoard> rankLeaderBoards = new List<RankLeaderBoard>();
}
[Serializable]
public class RankLeaderBoard: PooledData
{
    public int rank;
    public string name;
    public string walletAddress;
    public string profilePic;
    public int XP;
    public int gamePlayed;
    public int winRate;
    public int gameVol;
}





