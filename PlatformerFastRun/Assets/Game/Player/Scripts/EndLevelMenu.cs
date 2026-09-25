using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndLevelMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] CanvasGroup menuCanvasGroup;   // the panel holding Retry/Next buttons
    [SerializeField] float menuFadeInDuration = 0.5f;

    [Header("Scenes")]
    [SerializeField] string retrySceneName;         // leave empty to reload current scene
    [SerializeField] string nextLevelSceneName;

    [Header("Screen Transition")]
    [SerializeField] CanvasGroup fadeOverlay;        // full-screen black Image's CanvasGroup, alpha starts at 0
    [SerializeField] float sceneFadeDuration = 0.75f;

    bool isTransitioning;

    void Awake()
    {
        // Menu starts hidden
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }

    public void ShowMenu()
    {
        StartCoroutine(FadeInMenu());
    }

    IEnumerator FadeInMenu()
    {
        if (menuCanvasGroup == null) yield break;

        menuCanvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < menuFadeInDuration)
        {
            t += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Clamp01(t / menuFadeInDuration);
            yield return null;
        }
        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
    }

    // Hook this up to the Retry button's OnClick
    public void OnRetryPressed()
    {
        string target = string.IsNullOrEmpty(retrySceneName) ? SceneManager.GetActiveScene().name : retrySceneName;
        LoadScene(target);
    }

    // Hook this up to the Next Level button's OnClick
    public void OnNextLevelPressed()
    {
        LoadScene(nextLevelSceneName);
    }

    void LoadScene(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;
        isTransitioning = true;
        StartCoroutine(FadeThenLoad(sceneName));
    }

    IEnumerator FadeThenLoad(string sceneName)
    {
        if (menuCanvasGroup != null)
            menuCanvasGroup.interactable = false; // stop double-clicks mid-fade

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float t = 0f;
            while (t < sceneFadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t / sceneFadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        SceneManager.LoadScene(sceneName);
    }
}