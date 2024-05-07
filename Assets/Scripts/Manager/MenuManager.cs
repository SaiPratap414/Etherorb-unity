using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [SerializeField] Menu[] menus;

    [Header("Photon Variables")]
    public TimerScript countdownTimer;
    public TMP_Text nameText;

    [Header("MatchMaking")]
    public GameObject findingMatchPanel;
    public GameObject matchFoundPanel;
    [SerializeField] TMP_Text timerCounter;
    [SerializeField] TMP_Text matchFoundText;
    [SerializeField] Image matchFoundImage;
    [SerializeField] GameObject stopMatch;

    [Header("LoginPanel")]
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject loginPanel;
    [SerializeField] TMP_InputField Email_IF;


    [Header("PlayFab Username First Time..")]
    [SerializeField] GameObject newuserNamePanel;
    [SerializeField] TMP_InputField name_IF;

    [Header("Orb UI ")]
    [SerializeField] GameObject orbPrefab;
    [SerializeField] Transform content;

    [SerializeField] Button devLoginButton;
    [SerializeField] GameObject connectButtonPanel;
    [SerializeField] GameObject devLoginPanel;

    private string nameKey = "displayName";

    private AudioManager audioManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
    }

    private void Awake()
    {
        instance = this;
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("PhotonNetwork name -- " + PhotonNetwork.LocalPlayer.NickName);

        }
    }

    private void Start()
    {
        //if (PlayerPrefs.HasKey(nameKey))
        //{
        //    nameInputField.text = PlayerPrefs.GetString(nameKey);
        //}
        //else
        //{
        //    nameInputField.text = "Player" + Random.Range(0, 1000).ToString("0000");
        //}
        //UserNameValueChange();

        audioManager = AudioManager.Instance;

        audioManager.PlayAudio(AudioTag.BG);

        if (PlayfabConnet.instance.GetHasLogedIn)
        {
            nameText.text = PlayfabConnet.instance.PlayerName;
            OrbManager.instance.GetAllOrbDetails();
            OpenMenuId(2);
        }
        else if (UserPrefsManager.UserName !=string.Empty)
        {
            Email_IF.text = UserPrefsManager.UserName;
            LoginWithEmail();
        }
        else
        {
            OpenMenuId(0);
        }
    }

    private void Update()
    {
        //if (PlayfabConnet.instance.GetHasLogedIn)
        //{
        //    ConnectButton.SetActive(false);
        //    ConnectedButton.SetActive(true);
        //}

        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.IsConnected) return;

        if (PhotonNetwork.InRoom)
        {
            //timerCounter.text = TimerScript.instance.timerToStart.ToString("0");
        }

    }

    IEnumerator showLoadingScreenAndOpen(float sec, int menuId)
    {
        OpenMenuId(2);
        yield return new WaitForSeconds(sec);
        OpenMenuId(menuId);
    }

    public void OpenMenuId(int id)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menus[id].Open();
    }


    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }


    public void UserNameValueChange()
    {
        //PhotonConnector.instance.UsernameValueChange(nameInputField);
    }

    public void Start_Btn()
    {
        if (PlayfabConnet.instance.GetHasLogedIn)
        {
            FindAMatch();
        }
        else
        {
            OpenLoginPanel();
        }
    }


    public void FindAMatch()
    {
        audioManager.PlayAudio(AudioTag.Button);
        OpenMenuId(3);
        PhotonConnector.instance.FindMatch();
    }
    public void StopFindMatch()
    {
        audioManager.PlayAudio(AudioTag.Button);
        OpenMenuId(2);

        PhotonConnector.instance.StopSearch();
    }

    // Enable Match found Ui elements 
    public void MatchFoundUiEnbable()
    {
        findingMatchPanel.SetActive(false);
        matchFoundPanel.SetActive(true);
    }
    public void MatchFoundUiDisable()
    {
        findingMatchPanel.SetActive(true);
        matchFoundPanel.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 0)
        {
            //PlayFabManager
            SetMatchFoundProperties("FINDING MATCH",Color.black,true);
        }
        if (scene.buildIndex == 1)
        {
            Debug.Log("Scene 1 has been loaded!");
            PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void OpenLoginPanel()
    {
        OpenMenuId(0);
    }
    public void CloseLoginPanel()
    {
        Email_IF.text = string.Empty;
    }

    public void SetMatchFoundProperties(string textValue,Color bgColor,bool showStopMatch)
    {
        menus[3].GetComponent<Menu>().enabled = showStopMatch;
        matchFoundText.text = textValue;
        matchFoundImage.color = bgColor;
        matchFoundText.color = bgColor == Color.black ? Color.white : Color.black;
        stopMatch.SetActive(showStopMatch);
    }

    // this so that when we come to main menu the playfab connect refrence still stays...
    public void LoginWithWalletAddress(string addr)
    {
        if (addr.Length < 3) return;
        PlayfabConnet.instance.PlayFabLoginWithWalletId(addr);
    }

    // For dev Testing 
    public void LoginWithEmail()
    {
        string email = Email_IF.text;
        if(email.Length < 3) return;

        UserPrefsManager.UserName = email;
        Debug.Log("Logged In with Email:- " + email);
        PlayfabConnet.instance.PlayFabLoginWithWalletId(Email_IF.text);
    }

    public void DevLoginAction()
    {
        devLoginPanel.SetActive(true);
        connectButtonPanel.SetActive(false);
        audioManager.PlayAudio(AudioTag.Button);
    }    

    #region OrbSpawning Ui

    public GameObject SpawnOrbUI()
    {
        return Instantiate(orbPrefab, content);
    }

    #endregion


    #region New UserName set Panel

    //Sets the name in the PlayeFab (Button Func)
    public void SetNewUserName()
    {
        if (!string.IsNullOrEmpty(name_IF.text) && name_IF.text.Length > 2)
        {
            PlayfabConnet.instance.SetPlayerName(name_IF.text);
            nameText.text = name_IF.text;
            //UserNameValueChange();
            newuserNamePanel.SetActive(false);
        }
    }
    #endregion

}
