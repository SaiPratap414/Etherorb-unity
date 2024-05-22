using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MatchMaking : MonoBehaviour
{
    private float countdownTime = 15; // Set the countdown time here
    private bool timerActive = false;
    private double startTime;

    private Coroutine timerCoroutine;

    public void StartTimer()
    {
        PhotonNetwork.FetchServerTimestamp();
        startTime = Time.time; //PhotonNetwork.Time;
        timerActive = true;
        //timerCoroutine = StartCoroutine(StartTimerRoutine());
    }

    public void StopTimer()
    {
        timerActive = false;
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    IEnumerator StartTimerRoutine()
    {
        yield return new WaitForSeconds(countdownTime);
        MenuManager.instance.ShowRetryPanel();
        PhotonConnector.instance.StopSearch();

    }
}
