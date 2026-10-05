using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] float secondsPerCount = 1f;

    Coroutine countdownRoutine;

    public void RunCountdown(Action onComplete)
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownRoutine(onComplete));
    }

    IEnumerator CountdownRoutine(Action onComplete)
    {
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(secondsPerCount);
        }

        countdownText.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}