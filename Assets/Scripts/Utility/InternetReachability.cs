using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetReachability : MonoBehaviour
{
    private EtherOrbManager etherOrbManager;

    private bool isInternetAvailable = true;

    private void Start()
    {
        etherOrbManager = EtherOrbManager.Instance;
        StartCoroutine(InternetAvailbiltyCheck());
    }

    private IEnumerator InternetAvailbiltyCheck()
    {
        while (true)
        {
            CheckInternet();
            yield return new WaitForSeconds(1);
        }
    }

    private void CheckInternet()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable && isInternetAvailable)
        {
            Debug.Log("NotReachable--->");
            etherOrbManager.WarningPanel.ShowWarning("Check internet connection.", true);
            isInternetAvailable = false;
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && !isInternetAvailable)
        {
            etherOrbManager.WarningPanel.HideWarning();
            Debug.Log("ReachableViaCarrierDataNetwork--->");
            isInternetAvailable = true;
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork && !isInternetAvailable)
        {
            Debug.Log("ReachableViaCarrierDataNetwork--->");
            etherOrbManager.WarningPanel.HideWarning();
            isInternetAvailable = true;
        }
    }
}
