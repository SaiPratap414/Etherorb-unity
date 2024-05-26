using System.Collections;
using Photon.Pun;
using UnityEngine;

public class MatchMaking : MonoBehaviour
{
    private float countdownTime = 25; // Set the countdown time here

    private Coroutine timerCoroutine;

    public void StartTimer()
    {
        timerCoroutine = StartCoroutine(StartTimerRoutine());
    }

    public void StopTimer()
    {
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
