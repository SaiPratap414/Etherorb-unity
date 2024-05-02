using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindingMatchAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        animator.Play("Base Layer.IdleOrb");
    }
    private void OnDisable()
    {
        animator.StopPlayback();
    }
}
