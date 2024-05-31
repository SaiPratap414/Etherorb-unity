using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class OrbDetails
{
    public string id;
    [HideInInspector]
    public string image;
    public int Terra;
    public int Torrent;
    public int Blaze;

    public OrbDetails(string id, string image, int terra, int torrent, int blaze)
    {
        this.id = id;
        this.image = image;
        Terra = terra;
        Torrent = torrent;
        Blaze = blaze;
    }
}
[Serializable]
public class OrbList
{
    public List<OrbDetails> OrbDetails;
}

public class OrbManager : MonoBehaviour
{
    public static OrbManager instance;
    public NFTMeta OrbOwned;

    [SerializeField] NFTMetaData selectedOrb;
    [SerializeField] GameObject prevSelectedOrb;

    [SerializeField] TextAsset temp;

    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    [SerializeField] List<GameObject> UiObjects = new List<GameObject>();
    [SerializeField] Sprite placeholder;
    int selectedIndex = 0;

    private void Awake()
    {
        instance = this;
    }
    public void GetAllOrbDetails()
    {
        if(string.IsNullOrEmpty(EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress()))
            OrbOwned.OrbDetails.Clear();

        if (UiObjects.Count != 0)
        {
            foreach (GameObject obj in UiObjects) Destroy(obj);
        }
        UiObjects.Clear();
        sprites.Clear();
        OrbOwned = !string.IsNullOrEmpty(EtherOrbManager.Instance.WarningPanel.GetUserWalletAddress()) ? ApiManager.Instance.nftMetaData : JsonUtility.FromJson<NFTMeta>(temp.text);
        selectedOrb = OrbOwned.OrbDetails[0];
        selectedIndex = 0;
        bool firstElement = true;
        foreach (NFTMetaData orb in OrbOwned.OrbDetails)
        {
            GameObject obj = MenuManager.instance.SpawnOrbUI();
            UiObjects.Add(obj);
            Sprite sprite = GetSprite(orb.image_url);
            sprites.Add(sprite);
            obj.GetComponent<OrbMenuUi>().setObject(orb.image_url, orb.id ,orb.attributes.Terra,orb.attributes.Torrent, orb.attributes.Blaze);
            if (firstElement) 
            {
                obj.GetComponent<OrbMenuUi>().SelectThisObject();
                firstElement = false;
            }

        }
    }

    public void SetSelectedOrb(string name, GameObject obj)
    {
        if(prevSelectedOrb != null)
        {
            prevSelectedOrb.GetComponent<OrbMenuUi>().DeselectObject();
        }
        prevSelectedOrb = obj;
        selectedIndex = OrbOwned.OrbDetails.FindIndex(x => x.id == name);
        selectedOrb = OrbOwned.OrbDetails[selectedIndex];
        if(selectedOrb == null) 
        {
            Debug.LogError("NO Object Found with the Name");
        }
    }

    Sprite GetSprite(string img64)
    {
        //Texture2D tex = new(1, 1);
        //byte[] imgbytes = Convert.FromBase64String(img64);
        //tex.LoadImage(imgbytes, true);
        //Sprite sprite2d = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        //return sprite2d;
        return placeholder;
    }

    public NFTMetaData GetSelectedOrb()
    {
        return selectedOrb;
    }
    public Sprite GetSelectedSprite()
    {
        return sprites[selectedIndex];
    }


}
