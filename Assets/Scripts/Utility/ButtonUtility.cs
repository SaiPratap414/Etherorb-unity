using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonUtility : MonoBehaviour , IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Image Color")]
    [SerializeField] private Color hoverImageColor;
    [SerializeField] private Color normalImageColor;
    [SerializeField] private Color clickImageColor;

    [Space(10)]
    [Header("Text Color")]
    [SerializeField] private Color hoverTextColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color clickTextColor;

    [Space(10)]
    [Header("Image Spirites")]
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite normalSprite;

    [Space(10)]
    [Header("Configuration Settings")]
    [SerializeField] private bool changeSprite;
    [SerializeField] private bool hasClickState;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(hasClickState)
        {
            image.color = clickImageColor;
            text.color = clickTextColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (changeSprite)
        {
            image.sprite = hoverSprite;
        }
        else
        {
            image.color = hoverImageColor;
            text.color = hoverTextColor;
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (changeSprite)
        {
            image.sprite = normalSprite;
        }
        else
        {
            image.color = normalImageColor;
            text.color = normalTextColor;
        }
    }
}
