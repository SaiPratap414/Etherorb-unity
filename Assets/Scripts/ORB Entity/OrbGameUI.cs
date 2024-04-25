using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OrbGameUI : MonoBehaviour
{

    [SerializeField] Animator animator;
    [SerializeField] Image Orb_image;
    [SerializeField] TMP_Text Orb_name;
    [SerializeField] TMP_Text Terra_num;
    [SerializeField] TMP_Text Torrent_num;
    [SerializeField] TMP_Text Blaze_num;

    [SerializeField] ParticleSystem[] orbParticleSystem = new ParticleSystem[4];

    private int currentIndex = 0;

    
    private void Start()
    {
        //StartCoroutine(Test());
    }

    public void SetOrbStats(OrbDetails details)
    {
        Orb_name.SetText(details.id);
        Terra_num.SetText(details.Terra.ToString());
        Torrent_num.SetText(details.Torrent.ToString());
        Blaze_num.SetText(details.Blaze.ToString());
    }

    public void SetParticleGameObject(string animation)
    {
        animator.Play(animation);
    }

    IEnumerator Test()
    {
        float duration = 1f;
        animator.Play(GameManager.instance.GetOrbAnimationName[0]);
        yield return new WaitForSeconds(duration);
        animator.Play(GameManager.instance.GetOrbAnimationName[1]);
        yield return new WaitForSeconds(duration);
        animator.Play(GameManager.instance.GetOrbAnimationName[2]);
        yield return new WaitForSeconds(duration);
        animator.Play(GameManager.instance.GetOrbAnimationName[3]);
        yield return new WaitForSeconds(duration);
        animator.Play(GameManager.instance.GetOrbAnimationName[0]);
        yield return new WaitForSeconds(duration);
    }
}
