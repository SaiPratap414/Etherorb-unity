using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OrbMenuUi : MonoBehaviour
{
    [SerializeField] GameObject highlightBoarder;
    [SerializeField] Image BG;
    [SerializeField] Image Orb_image;
    [SerializeField] TMP_Text Orb_name;
    [SerializeField] TMP_Text Terra_num;
    [SerializeField] TMP_Text Torrent_num;
    [SerializeField] TMP_Text Blaze_num;


    [SerializeField] Button selectButton;
    [SerializeField] TMP_Text buttonName;

    [SerializeField] Color selectedColor;
    [SerializeField] Color deSelectedColor;

    private Vector3 selectedScale = new Vector3(1.05f, 1.05f, 1.05f);

    bool isSelected = false;

    private void Start()
    {
        selectButton.onClick.AddListener(delegate { OnSelected(); });
    }

    public void setObject(string imageUrl, string name, int Terra, int Torrent, int Blaze)
    {
        //Orb_image.sprite = image64;
        Orb_name.text = name;
        Terra_num.text = Terra.ToString();
        Torrent_num.text = Torrent.ToString();
        Blaze_num.text = Blaze.ToString();

        if(!string.IsNullOrEmpty(EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress()))
            DownloadImage(imageUrl);
    }

    private void DownloadImage(string url)
    {
        Debug.Log(url);
        ApiManager.Instance.DownloadImage(url,string.Empty,OnDownLoadComplete);
    }

    private void OnDownLoadComplete(byte[] imageData)
    {
        Texture2D texture2D = new Texture2D(2, 2);
        texture2D.LoadImage(imageData);
        Orb_image.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
    }

    public void SelectThisObject()
    {
        OnSelected();
    }
    public void DeselectObject()
    {
        selectButton.interactable = true;
        buttonName.text = string.Empty;
        BG.color = deSelectedColor;
        transform.localScale = Vector3.one;
        isSelected = false;
        highlightBoarder.SetActive(false);
    }

    void OnSelected()
    {
        if (isSelected) return;

        OrbManager.instance.SetSelectedOrb(Orb_name.text, this.gameObject);
        selectButton.interactable = false;
        BG.color = selectedColor;
        buttonName.text = "SELECTED";
        highlightBoarder.SetActive(true);
        transform.localScale = selectedScale;
        isSelected = true;

    }
}
