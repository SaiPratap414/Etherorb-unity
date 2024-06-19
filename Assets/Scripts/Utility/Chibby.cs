using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chibby : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void PlayAnimation(string anim)
    {
        animator.SetTrigger(anim);
    }
    
}
