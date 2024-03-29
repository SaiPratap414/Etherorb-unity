using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OrbMenuUi : MonoBehaviour
{
    [SerializeField] Image Orb_image;
    [SerializeField] TMP_Text Orb_name;
    [SerializeField] TMP_Text Terra_num;
    [SerializeField] TMP_Text Torrent_num;
    [SerializeField] TMP_Text Blaze_num;


    [SerializeField] Button selectButton;
    [SerializeField] TMP_Text buttonName;

    bool isSelected = false;

    private void Start()
    {
        selectButton.onClick.AddListener(delegate { OnSelected(); });
    }

    public void setObject(Sprite image64, string name, int Terra, int Torrent, int Blaze)
    {
        Orb_image.sprite = image64;
        Orb_name.text = name;
        Terra_num.text = Terra.ToString();
        Torrent_num.text = Torrent.ToString();
        Blaze_num.text = Blaze.ToString();
    }


    public void SelectThisObject()
    {
        OnSelected();
    }
    public void DeselectObject()
    {
        selectButton.interactable = true;
        buttonName.text = "Select";
        isSelected = false;
    }

    void OnSelected()
    {
        if (isSelected) return;

        OrbManager.instance.SetSelectedOrb(Orb_name.text, this.gameObject);
        selectButton.interactable = false;
        buttonName.text = "Selected";

        isSelected = true;

    }



}
