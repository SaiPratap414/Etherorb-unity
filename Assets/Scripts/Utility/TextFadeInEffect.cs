using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextFadeInEffect : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Vector3 initialPos;
    private float moveY = 20;
    private float duration = 1.6f;
    private Color initialColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        DOTween.Init();
        initialColor = textMesh.color;
    }

    public void ShowEffect(Color finalColor)
    {
        initialColor = finalColor;
        finalColor.a = 0;
        initialPos = textMesh.transform.position;
        textMesh.transform.DOMoveY((initialPos.y + moveY), duration).SetEase(Ease.OutQuint).OnComplete(()=> {
            textMesh.transform.position = initialPos;
        });

        DOVirtual.Color(initialColor, finalColor, duration, (value) =>
        {
            textMesh.color = value;
        }).OnComplete(()=> {
            
        });
    }
}
