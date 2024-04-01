using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OrbGameUI : MonoBehaviour
{

    [SerializeField] Image Orb_image;
    [SerializeField] TMP_Text Orb_name;
    [SerializeField] TMP_Text Terra_num;
    [SerializeField] TMP_Text Torrent_num;
    [SerializeField] TMP_Text Blaze_num;

    


    private void Start()
    {
        
    }

    public void SetOrbStats(OrbDetails details)
    {
        Orb_name.SetText(details.id);
        Terra_num.SetText(details.Terra.ToString());
        Torrent_num.SetText(details.Torrent.ToString());
        Blaze_num.SetText(details.Blaze.ToString());
    }
   

}
