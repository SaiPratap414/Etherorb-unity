using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioTag
{
    BG,
    Button,
    Hover,
    ReadyButton,
    Timer15,
    PointGain,
    PointLose,
    WinScreen,
    LoseScreen,
    Error,
    Tie,
    TERRA,
    TORRENT,
    BLAZE,
    None
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [SerializeField] private AudioScriptable audioConfig;
    [SerializeField] private AudioSource bgSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource timerSource;

    public static AudioManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;
    }
    public void PlayAudio(AudioTag audioTag)
    {
        Audios audios = audioConfig.audios.Find(x => x.AudioTag.Equals(audioTag));

        if (audios != null)
        {
            if (audios.AudioTag == AudioTag.BG)
            {
                PlayAudio(bgSource, audios);
            }
            else if (audios.AudioTag == AudioTag.Timer15)
            {
                PlayAudio(timerSource, audios);
            }
            else
            {
                PlayAudio(sfxSource, audios);
            }
        }
    }

    private void PlayAudio(AudioSource source,Audios audioScriptable)
    {
        source.clip = audioScriptable.AudioClip;
        source.loop = audioScriptable.Loop;
        source.volume = audioScriptable.volume;
        source.Play();
    }

    public void StopTimerSound()
    {
        timerSource.Stop();
    }
    public void StopMusic()
    {
        if (bgSource != null)
        {
            bgSource.Stop();
            bgSource.enabled = false;
        }
    }
    public void StopSfxSound()
    {
        if (sfxSource != null && timerSource != null)
        {
            sfxSource.Stop();
            timerSource.Stop();
            sfxSource.enabled = false;
            timerSource.enabled = false;
        }
    }

    public void PlayMusic()
    {
        if (bgSource != null)
        {
            bgSource.enabled = true;
            bgSource.Play();
        }
    }
    public void PlaySfx()
    {
        if (sfxSource != null && timerSource != null)
        {
            sfxSource.enabled = true;
            timerSource.enabled = true;
            sfxSource.Play();
            timerSource.Play();
        }
    }
}
