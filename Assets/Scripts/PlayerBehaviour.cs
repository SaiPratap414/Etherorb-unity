using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    PhotonView pv;
    TMP_Text nameText;

    
    

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            

        }
    }



   
    

    
}
