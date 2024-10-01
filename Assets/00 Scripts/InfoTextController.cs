using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoTextController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] TextMeshProUGUI display;
    [Header("Variables")]
    [SerializeField][Range(1f, 4f)] float tempTextDur;
    [SerializeField][Range(0.02f, 0.2f)] float charWritingDelay;

    string permText;
    Coroutine currentCoroutine = null;

    public void WritePermanentText(string text)
    {
        permText = text;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(Co_WriteText(text));
    }

    public void WriteTempText(string text)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(Co_DisplayTemporaryText(text));
    }

    private IEnumerator Co_DisplayTemporaryText(string tempText)
    {
        StartCoroutine(Co_WriteText(tempText));

        float timeRemaining = tempTextDur;
        while (true)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining < 0)
            {
                WritePermanentText(permText);
                break;
            }

            yield return null;
        }
    }

    private IEnumerator Co_WriteText(string text)
    {
        display.text = text;
        display.maxVisibleCharacters = 0;

        float timeToNextLetter = charWritingDelay;
        while (true)
        { 
            timeToNextLetter -= Time.deltaTime;

            if (timeToNextLetter < 0)
            {
                display.maxVisibleCharacters++;
                timeToNextLetter = charWritingDelay;
                //TODO: add sound for typing
            }

            if (display.maxVisibleCharacters == display.text.Length)
            {
                break;
            }

            yield return null;
        }
    }
}
