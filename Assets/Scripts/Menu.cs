using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool open;
    [Header("If Only has Logo (only for loading panel)")]
    public bool isLoading;
    public GameObject GameLogo;
    public float rotateSpeed;
    private bool startRotating;


    private void OnEnable()
    {
        startRotating = true;
    }
    private void OnDisable()
    {
        startRotating = false;
        if (isLoading)
            GameLogo.transform.rotation = Quaternion.identity;
    }
    private void Update()
    {
        if (GameLogo)
        {
            if (startRotating)
            {
                GameLogo.transform.localRotation *= Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);
            }
        }

    }

    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }
}
