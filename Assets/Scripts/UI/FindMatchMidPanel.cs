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

    private Coroutine moveRoutine;

    public void StartPanelMove()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine =StartCoroutine(StartMovementPanel());
    }

    private IEnumerator StartMovementPanel()
    {
        SetDotsState(0);
        yield return new WaitForSeconds(panelStayDuration);
        winningRT.DOLocalMoveX(winningRT.anchoredPosition3D.x - 1500, 1f).OnComplete(()=> {
            winningRT.anchoredPosition3D = new Vector2(1500, 0);
            SetDotsState(1);
        });
        traitsRT.DOLocalMoveX(traitsRT.anchoredPosition3D.x-1500, 1f);

        yield return new WaitForSeconds(panelStayDuration);
        traitsRT.DOLocalMoveX(traitsRT.anchoredPosition3D.x - 1500, 1f).OnComplete(() =>
        {
            traitsRT.anchoredPosition3D = new Vector2(1500, 0);
            SetDotsState(2);
        });
        battleRT.DOLocalMoveX(battleRT.anchoredPosition3D.x - 1500, 1f);

        yield return new WaitForSeconds(panelStayDuration);
        battleRT.DOLocalMoveX(battleRT.anchoredPosition.x - 1500, 1f).OnComplete(() =>
        {
            battleRT.anchoredPosition = new Vector2(1500, 0);
            SetDotsState(0);
        });
        winningRT.DOLocalMoveX(0, 1f);

        StartCoroutine(StartMovementPanel());
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
