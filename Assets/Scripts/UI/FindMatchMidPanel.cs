using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FindMatchMidPanel : MonoBehaviour
{
    [SerializeField] private RectTransform winningRT;
    [SerializeField] private RectTransform traitsRT;
    [SerializeField] private RectTransform battleRT;

    [SerializeField] private List<Image> dots;

    [SerializeField] private Color dotSelectedColor;
    [SerializeField] private Color dotDeSelectedColor;

    private float panelStayDuration = 4.5f;
    private List<RectTransform> rectTransforms = new List<RectTransform>();

    private Coroutine moveRoutine;

    public void StartPanelMove()
    {
        rectTransforms.Clear();
        rectTransforms.Add(winningRT);
        rectTransforms.Add(traitsRT);
        rectTransforms.Add(battleRT);
        Shuffle(rectTransforms);
        SetScreensPositions();

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine =StartCoroutine(StartMovementPanel());
    }
    private void SetScreensPositions()
    {
        rectTransforms[0].DOLocalMoveX(0, 0f);
        rectTransforms[1].anchoredPosition3D = new Vector2(1500, 0);
        rectTransforms[2].anchoredPosition3D = new Vector2(1500, 0);
    }
    private IEnumerator StartMovementPanel()
    {
        SetDotsState(0);
        yield return new WaitForSeconds(panelStayDuration);
        rectTransforms[0].DOLocalMoveX(rectTransforms[0].anchoredPosition3D.x - 1500, 1f).OnComplete(()=> {
            rectTransforms[0].anchoredPosition3D = new Vector2(1500, 0);
            SetDotsState(1);
        });
        rectTransforms[1].DOLocalMoveX(rectTransforms[1].anchoredPosition3D.x-1500, 1f);

        yield return new WaitForSeconds(panelStayDuration);
        rectTransforms[1].DOLocalMoveX(rectTransforms[1].anchoredPosition3D.x - 1500, 1f).OnComplete(() =>
        {
            rectTransforms[1].anchoredPosition3D = new Vector2(1500, 0);
            SetDotsState(2);
        });
        rectTransforms[2].DOLocalMoveX(rectTransforms[2].anchoredPosition3D.x - 1500, 1f);

        yield return new WaitForSeconds(panelStayDuration);
        rectTransforms[2].DOLocalMoveX(rectTransforms[2].anchoredPosition.x - 1500, 1f).OnComplete(() =>
        {
            rectTransforms[2].anchoredPosition = new Vector2(1500, 0);
            SetDotsState(0);
        });
        rectTransforms[0].DOLocalMoveX(0, 1f);

        StartCoroutine(StartMovementPanel());
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void SetDotsState(int index)
    {
        foreach (var item in dots)
        {
            item.color = dotDeSelectedColor;
        }
        dots[index].color = dotSelectedColor;
    }
}
