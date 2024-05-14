using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class RoundTimer : MonoBehaviour
{

    [SerializeField] TMP_Text TimerTxt;

    [SerializeField] bool inRound;


    [SerializeField] float timerToStart;
    [SerializeField] float fullRoundTimer;

    [SerializeField] float maxRoundTime;

    [SerializeField] bool roundDone;

    PhotonView myPhotonView;

    [SerializeField] private bool rematchTimer;



    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
        fullRoundTimer = maxRoundTime;
        timerToStart = maxRoundTime;
        if (PhotonNetwork.IsMasterClient)
        {
            RPC_Time();
        }
    }


    private void Update()
    {

        if (!inRound)
            return;

        CounterLogic();
    }

    public void RPC_Time()
    {
        myPhotonView.RPC(nameof(RPC_SendTimer), RpcTarget.MasterClient, timerToStart);
    }

    public void setRoundBool(bool set)
    {
        myPhotonView.RPC(nameof(PRC_roundTrue), RpcTarget.All, set);
    }

    [PunRPC]
    void PRC_roundTrue(bool set)
    {
        inRound = set;
    }


    [PunRPC]
    void RPC_SendTimer(float timeIn)
    {
        timerToStart = timeIn;
        fullRoundTimer = timeIn;
        //if (timeIn < fullGameTimer)
        //{
        //    fullGameTimer = timeIn;
        //}
    }


    void CounterLogic()
    {
        if (inRound)
        {
            fullRoundTimer -= Time.deltaTime;
            timerToStart -= fullRoundTimer;
            TimerTxt.text = fullRoundTimer.ToString("00");

            if (fullRoundTimer <= 0f)
            {
                inRound = false;
                TimerTxt.text = "";
                if(rematchTimer)
                {
                    EventManager.Instance.OnRematchTimerCompletedInvoke();
                }
                else
                {
                    GameManager.instance.setGameState = GameState.RoundEnd;
                }
            }
        }
    }


    public void resetTimer()
    {
        myPhotonView.RPC(nameof(RPC_resetTimer), RpcTarget.All);
    }

    [PunRPC]
    void RPC_resetTimer()
    {
        fullRoundTimer = maxRoundTime;
        timerToStart = maxRoundTime;
        inRound = true;
        Debug.Log("Timer has been reseted");
    }

    public void StopTimer()
    {
        myPhotonView.RPC(nameof(RPC_StopTime), RpcTarget.All);
    }

    [PunRPC]
    void RPC_StopTime()
    {
        inRound = false;
        TimerTxt.text = "";
    }
}
