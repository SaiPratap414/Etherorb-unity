using System.Collections.Generic;
using UnityEngine;
using System;

public enum CardType
{
    BLAZE,
    TERRA,
    TORRENT,
    NONE
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "EtherOrb/Create GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public Color lostColor;
    public Color winColor;
    public List<LostAnimationName> lostAnimationNames;
    public List<WinVFXNames> winVFXNames;

    public List<string> statisticName = new List<string>();
    public List<Gradient> gradients = new List<Gradient>();
}

[Serializable]
public class LostAnimationName
{
    public CardType type;
    public string lostAnimationName;
}

[Serializable]
public class WinVFXNames
{
    public CardType type;
    public string zapAnimationName;
    public string impactAnimationName;
}