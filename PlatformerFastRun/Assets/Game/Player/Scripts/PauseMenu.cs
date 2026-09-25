using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup pauseCanvasGroup; // panel with Resume/Retry/Menu buttons
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerHealth playerHealth;

    [Header("Pause Key (optional)")]
    [SerializeField] bool allowKeyToggle = true;
    [SerializeField] Key pauseKey = Key.Escape;

    [Header("Scenes")]
    [SerializeField] string menuSceneName;

    bool isPaused;

    void Awake()
    {
        SetMenuVisible(false);
    }

    void Update()
    {
        if (allowKeyToggle && Keyboard.current != null && Keyboard.current[pauseKey].wasPressedThisFrame)
            TogglePause();
    }

    // Hook this up to your Pause button's OnClick
    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerHealth != null) playerHealth.enabled = false;

        SetMenuVisible(true);
    }

    // Hook this up to your Resume button's OnClick
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerHealth != null) playerHealth.enabled = true;

        SetMenuVisible(false);
    }

    // Hook this up to the pause menu's Retry button
    public void OnRetryPressed()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Hook this up to the pause menu's "Back to Menu" button
    public void OnMenuPressed()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene(menuSceneName);
    }

    void SetMenuVisible(bool visible)
    {
        if (pauseCanvasGroup == null) return;
        pauseCanvasGroup.alpha = visible ? 1f : 0f;
        pauseCanvasGroup.interactable = visible;
        pauseCanvasGroup.blocksRaycasts = visible;
    }
}