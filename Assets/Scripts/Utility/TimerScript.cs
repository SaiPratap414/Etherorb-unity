using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class TimerScript : MonoBehaviourPunCallbacks
{
    public const string CountdownStartTime = "StartTime";

    [Header("Countdown time in seconds")]
    public float Countdown = 5.0f;

    private bool isTimerRunning;

    private int startTime;
    private float cachedTimeRemaining;

    [Header("Reference to a Text component for visualizing the countdown")]
    public TMP_Text Text;


    public void Start()
    {
        if (this.Text == null) Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
    }

    public override void OnEnable()
    {
        Debug.Log("OnEnable CountdownTimer");
        base.OnEnable();
        // the starttime may already be in the props. look it up.
        Initialize();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Debug.Log("OnDisable CountdownTimer");
    }


    public void Update()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount > 2)
            return;

        if (!this.isTimerRunning) return;

        cachedTimeRemaining = TimeRemaining();
        Text.SetText($"STARTS IN {cachedTimeRemaining:n0} SECS");

        if (cachedTimeRemaining > 0.0f) return;

        OnTimerEnds();
    }


    private void OnTimerRuns()
    {
        MenuManager.instance.SetMatchFoundProperties("MATCH FOUND",Color.white,false);
        this.isTimerRunning = true;
    }

    private void OnTimerEnds()
    {
        this.isTimerRunning = false;
        this.Text.text = string.Empty;
        cachedTimeRemaining = 0;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonConnector.instance.StartGame();
            return;
        }
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenuId(2);
    }


    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("CountdownTimer.OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());
        Initialize();
    }


    private void Initialize()
    {
        int propStartTime;
        if (TryGetStartTime(out propStartTime))
        {
            this.startTime = propStartTime;
            Debug.Log("Initialize sets StartTime " + this.startTime + " server time now: " + PhotonNetwork.ServerTimestamp + " remain: " + TimeRemaining());


            this.isTimerRunning = TimeRemaining() > 0;

            if (this.isTimerRunning)
                OnTimerRuns();
            else
                OnTimerEnds();
        }
    }


    private float TimeRemaining()
    {
        int timer = PhotonNetwork.ServerTimestamp - this.startTime;
        return this.Countdown - timer / 1000f;
    }


    public static bool TryGetStartTime(out int startTimestamp)
    {
        startTimestamp = PhotonNetwork.ServerTimestamp;

        object startTimeFromProps;
        if (PhotonNetwork.CurrentRoom !=null &&  PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CountdownStartTime, out startTimeFromProps))
        {
            Debug.Log(startTimeFromProps);
            startTimestamp = (int)startTimeFromProps;
            return true;
        }

        return false;
    }
    public void StartTimer()
    {
        SetStartTime();
    }

    public static void SetStartTime()
    {
        int startTime = 0;
        bool wasSet = TryGetStartTime(out startTime);

        Hashtable props = new Hashtable
            {
                {CountdownTimer.CountdownStartTime, (int)PhotonNetwork.ServerTimestamp}
            };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);


        Debug.Log("Set Custom Props for Time: " + props.ToStringFull() + " wasSet: " + wasSet);
    }
}
