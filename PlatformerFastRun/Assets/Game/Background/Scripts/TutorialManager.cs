using System.Collections;
using UnityEngine;

// Put this on a manager object in your scene (e.g. an empty "TutorialManager" GameObject).
// It keeps track of whichever tutorial panel is currently showing and fades between them.
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    private CanvasGroup currentPanel;
    private Coroutine fadeRoutine;

    void Awake()
    {
        // simple singleton so triggers can call TutorialManager.Instance from anywhere
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Call this from a trigger to show a new panel (and fade out whatever's currently showing)
    public void ShowPanel(CanvasGroup newPanel)
    {
        if (newPanel == currentPanel) return; // already showing this one, do nothing

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(currentPanel, newPanel));
        currentPanel = newPanel;
    }

    private IEnumerator FadeRoutine(CanvasGroup oldPanel, CanvasGroup newPanel)
    {
        // make sure the new panel starts invisible and non-interactable before we fade it in
        if (newPanel != null)
        {
            newPanel.alpha = 0f;
            newPanel.gameObject.SetActive(true);
        }

        float t = 0f;
        float oldStartAlpha = oldPanel != null ? oldPanel.alpha : 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float pct = t / fadeDuration;

            if (oldPanel != null)
                oldPanel.alpha = Mathf.Lerp(oldStartAlpha, 0f, pct);

            if (newPanel != null)
                newPanel.alpha = Mathf.Lerp(0f, 1f, pct);

            yield return null;
        }

        // snap to final values just in case
        if (oldPanel != null)
        {
            oldPanel.alpha = 0f;
            oldPanel.interactable = false;
            oldPanel.blocksRaycasts = false;
            oldPanel.gameObject.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.alpha = 1f;
            newPanel.interactable = true;
            newPanel.blocksRaycasts = true;
        }
    }
}