using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    public bool open;
    [Header("If Only has Logo (only for loading panel)")]
    public bool isLoading;
    public GameObject GameLogo;
    public float rotateSpeed;
    private bool startRotating;
    [SerializeField] private TextMeshProUGUI loadText;
    [SerializeField] private string msgForLoading;

    private void OnEnable()
    {
        startRotating = true;
    }
    private void OnDisable()
    {
        startRotating = false;
        StopAllCoroutines();
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

    IEnumerator ShowLoadingText()
    {
        while(startRotating && loadText != null)
        {
            loadText.text = msgForLoading + ".";
            yield return new WaitForSeconds(0.3f);
            loadText.text = msgForLoading+ "..";
            yield return new WaitForSeconds(0.3f);
            loadText.text = msgForLoading+"...";
            yield return new WaitForSeconds(0.3f);
            
        }
    }

    public void StartLoadingText()
    {
        StartCoroutine(ShowLoadingText());
    }


    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
        StartCoroutine(ShowLoadingText());
    }

    public void Close()
    {
        try
        {
            open = false;
            gameObject.SetActive(false);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
