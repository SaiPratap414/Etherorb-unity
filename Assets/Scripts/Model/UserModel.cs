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