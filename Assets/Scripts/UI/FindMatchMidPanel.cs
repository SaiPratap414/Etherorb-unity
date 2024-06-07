using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindMatchMidPanel : MonoBehaviour
{
    [SerializeField] private List<Image> dots;

    [SerializeField] private Color dotSelectedColor;
    [SerializeField] private Color dotDeSelectedColor;

    [SerializeField] private Transform content;

    public void StartPanelMove()
    {
    }

    public void SetDotsState(int index)
    {
        foreach (var item in dots)
        {
            item.color = dotDeSelectedColor;
        }
        dots[index].color = dotSelectedColor;
    }
}
