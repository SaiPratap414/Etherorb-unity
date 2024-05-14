using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager
{
    private static EventManager instance;

    public static EventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EventManager();
            }
            return instance;
         }
    }
    public event Action OnNoButtonAction;
    public void OnNoButtonActionInvoke() => OnNoButtonAction?.Invoke();

    public event Action OnYesButtonAction;
    public void OnYesButtonActionInvoke() => OnNoButtonAction?.Invoke();

    public event Action OnRematchTimerCompleted;
    public void OnRematchTimerCompletedInvoke() => OnRematchTimerCompleted?.Invoke();
}
