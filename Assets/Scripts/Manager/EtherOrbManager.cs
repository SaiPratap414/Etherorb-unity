using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EtherOrbManager : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private WarningPanel warningPanel;

    public AudioManager AudioManager { get { return audioManager; } }
    public WarningPanel WarningPanel { get { return warningPanel; } }

    private static EtherOrbManager instance;
    public static EtherOrbManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

}
