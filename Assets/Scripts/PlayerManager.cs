using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;

    GameObject Player;
    [SerializeField] int ActorNum;

    [SerializeField] int currentSelectedOptions = 0;
    [SerializeField] int TotalScore = 0;
    [SerializeField] OrbGameUI gameUI;

    OrbDetails OrbDetails;
    public int TerraValue { get { return OrbDetails.Terra; } }
    public int TorrentValue { get { return OrbDetails.Torrent; } }
    public int BlazeValue { get { return OrbDetails.Blaze; } }

    public OrbDetails getOrbDetails {  get { return OrbDetails; } }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        ActorNum = pv.OwnerActorNr;
    }

    private void Start()
    {
        gameUI = pv.OwnerActorNr == 1 ? GameManager.instance.GetPlayerOrb1 : GameManager.instance.GetPlayerOrb2;
        if (pv.IsMine)
        {
            if(ActorNum == 1)
            {
                GameManager.instance.GetOptionButtonsPlayer1[0].onClick.AddListener(delegate { ChangeTheOption(1); });
                GameManager.instance.GetOptionButtonsPlayer1[1].onClick.AddListener(delegate { ChangeTheOption(2); });
                GameManager.instance.GetOptionButtonsPlayer1[2].onClick.AddListener(delegate { ChangeTheOption(3); });

                foreach (var item in GameManager.instance.GetOptionButtonsPlayer2)
                {
                    item.GetComponent<ButtonUtility>().enabled = false;
                }
            }
            else if (ActorNum == 2)
            {
                GameManager.instance.GetOptionButtonsPlayer2[0].onClick.AddListener(delegate { ChangeTheOption(1); });
                GameManager.instance.GetOptionButtonsPlayer2[1].onClick.AddListener(delegate { ChangeTheOption(2); });
                GameManager.instance.GetOptionButtonsPlayer2[2].onClick.AddListener(delegate { ChangeTheOption(3); });
                foreach (var item in GameManager.instance.GetOptionButtonsPlayer1)
                {
                    item.GetComponent<ButtonUtility>().enabled = false;
                }
            }
            
            OrbDetails = OrbManager.instance.GetSelectedOrb();
            pv.RPC(nameof(RPC_OrbStatSync), RpcTarget.OthersBuffered, OrbDetails.id, OrbDetails.image, OrbDetails.Terra, OrbDetails.Torrent, OrbDetails.Blaze);
            pv.RPC(nameof(RPC_SendThisToGameManager), RpcTarget.All, ActorNum);
        }
        GameManager.instance.SetPlayerReady(ActorNum);
    }


    private void Update()
    {
        if (!pv.IsMine)
            return;
        if (PhotonNetwork.LocalPlayer.ActorNumber == pv.OwnerActorNr)
        {
            TotalScore = GameManager.instance.GetPlayerScore(pv.OwnerActorNr);
        }
    }

    void CreatePlayerObj()
    {
    }
    void ChangeTheOption(int num)
    {
        pv.RPC(nameof(RPC_ChangeTheOption), RpcTarget.All, num);
    }

    [PunRPC]
    void RPC_SendThisToGameManager(int Playernum)
    {
        if(Playernum == 1)
            GameManager.instance.playerManager1 = this;
        if(Playernum == 2)
            GameManager.instance.playerManager2 = this;
    }


    [PunRPC]
    void RPC_ChangeTheOption(int num)
    {

        GameManager.instance.SetParticleGameObject(GameManager.instance.GetOrbAnimationName[num], pv.OwnerActorNr);
        //GameManager.instance.ShowParticleGameObjects(num, pv.OwnerActorNr);
        GameManager.instance.InputChoise(num, pv.OwnerActorNr);
        currentSelectedOptions = num;
    }



    [PunRPC]
    void RPC_OrbStatSync(string id, string image, int terra, int torrent, int blaze)
    {
        OrbDetails = new OrbDetails(id,image, terra, torrent, blaze);
    }


    private void OnDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

}
