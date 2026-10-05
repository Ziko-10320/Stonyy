using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
public class MenuParallax : MonoBehaviour
{
    public float offsetMultiplier = 1f;
    public float smoothTime = .3f;
    [Header("Fade")]
    [SerializeField] CanvasGroup fadeOverlay; // full-screen black Image's CanvasGroup, alpha starts at 0
    [SerializeField] float fadeDuration = 0.75f;
    private Vector2 startPosition;
    private Vector3 velocity;

    private void Start()
    {
        startPosition = transform.position;
    }
    public void OnPlayPressed()
    {
        StartCoroutine(FadeThenLoad(0));
    }

    IEnumerator FadeThenLoad(int buildIndex)
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        SceneManager.LoadScene(buildIndex);
    }

    public void OnQuitPressed()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void Update()
    {
        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector2 offset = Camera.main.ScreenToViewportPoint(mousePos);
        transform.position = Vector3.SmoothDamp(transform.position, startPosition + (offset * offsetMultiplier), ref velocity, smoothTime);
    }
}