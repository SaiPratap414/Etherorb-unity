using UnityEngine;

public static class UserPrefsManager
{
    private const string userName = nameof(userName);
    //private const string hasLogedIn = nameof(hasLogedIn);

    internal static string UserName
    {
        get
        {
            if (!PlayerPrefs.HasKey(userName))
                UserName = string.Empty;
            return PlayerPrefs.GetString(userName);
        }
        set
        {
            PlayerPrefs.SetString(userName, value);
            PlayerPrefs.Save();
        }
    }

    //internal static int GetHasLogedIn
    //{
    //    get
    //    {
    //        if (!PlayerPrefs.HasKey(hasLogedIn))
    //            GetHasLogedIn = 0;
    //        return PlayerPrefs.GetInt(hasLogedIn);
    //    }
    //    set
    //    {
    //        PlayerPrefs.SetInt(hasLogedIn, value);
    //        PlayerPrefs.Save();
    //    }
    //}
}
