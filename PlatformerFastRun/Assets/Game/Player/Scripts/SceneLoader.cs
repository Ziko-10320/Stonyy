using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Screen Transition")]
    [SerializeField] CanvasGroup fadeOverlay;
    [SerializeField] float fadeDuration = 0.5f;

    bool isTransitioning;
    Coroutine activeFadeRoutine; // add this

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Screen.SetResolution(1920, 1080, true);
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;
        isTransitioning = true;
        Time.timeScale = 1f;

        if (activeFadeRoutine != null)
            StopCoroutine(activeFadeRoutine); // add this — kill any leftover fade-in first

        activeFadeRoutine = StartCoroutine(FadeThenLoad(sceneName));
    }

    IEnumerator FadeThenLoad(string sceneName)
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
        isTransitioning = false;

        if (fadeOverlay != null)
            activeFadeRoutine = StartCoroutine(FadeIn()); // add this — track it instead of fire-and-forget
    }

    IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;
        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
        activeFadeRoutine = null;
    }
}