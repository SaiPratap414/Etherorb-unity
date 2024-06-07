using System.Collections;
using UnityEngine;
using TMPro;

public class ReMatchTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float rematchTime = 15;
    private Coroutine timeCoroutine;

    public void StartTimer()
    {
        rematchTime = 15;
        StopTimer();
        timeCoroutine = StartCoroutine(StartTicks());
    }
    public void StopTimer()
    {
        if (timeCoroutine !=null)
            StopCoroutine(timeCoroutine);
    }
    private IEnumerator StartTicks()
    {
        while(rematchTime > 0)
        {
            timerText.text = ((int)rematchTime).ToString();
            rematchTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        EventManager.Instance.OnRematchTimerCompletedInvoke();
    }
}
