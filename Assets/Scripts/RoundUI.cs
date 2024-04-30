using UnityEngine;
using UnityEngine.UI;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite drawSprite;

    public void SetUpRoundUI(bool won,bool isDraw=false)
    {
        if (isDraw)
        {
            image.sprite = drawSprite;
        }
        else
        {
            image.color = won ? Color.green : Color.red;
        }
    }
}
