using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TextRevealEffect : MonoBehaviour
{

    public float revealSpeed = 0.1f; // Speed at which letters are revealed
    private TextMeshProUGUI textMeshPro;
    private string fullText;
    private string currentText = "";

    private float revealTimer = 0f;
    private int currentIndex = 0;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = ""; // Clear the text initially
    }
    public void StartEffect(string msg)
    {
        fullText = msg;
        StartCoroutine(StartRevealing());
    }
    private IEnumerator StartRevealing()
    {
        currentIndex = 0;
        while (currentIndex < fullText.Length)
        {
            revealTimer += Time.deltaTime;

            if (revealTimer >= revealSpeed)
            {
                currentIndex++;
                currentText = fullText.Substring(0, currentIndex);
                textMeshPro.text = currentText;
                revealTimer = 0f;
            }
            yield return null;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TextRevealEffect))]
public class customButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextRevealEffect myScript = (TextRevealEffect)target;
        if (GUILayout.Button("Play Effect"))
        {
            //myScript.StartEffect();
        }
    }

}
#endif
