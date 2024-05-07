using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Audios", menuName = "Audio/Create Audio File", order = 1)]
public class AudioScriptable : ScriptableObject
{
    public List<Audios> audios;
}

[Serializable]
public class Audios
{
    [SerializeField] private AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1;
    [SerializeField] private AudioTag audioTag;

    [SerializeField] private bool loop;
    [SerializeField] private bool isMusic;

    public AudioTag AudioTag { get { return audioTag; } }

    public AudioClip AudioClip { get { return audioClip; } }

    public bool Loop { get { return loop; } }
}