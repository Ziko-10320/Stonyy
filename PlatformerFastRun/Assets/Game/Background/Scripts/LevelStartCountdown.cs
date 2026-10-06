using UnityEngine;

public class LevelStartCountdown : MonoBehaviour
{
    [SerializeField] CountdownController countdownController;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerSFX playerSFX;
    void Start()
    {
        playerMovement.enabled = false;
        Time.timeScale = 0f;

        if (playerSFX != null)
        {
            playerSFX.SetRunLoop(false);
            playerSFX.SetSlideLoop(false);
        }

        countdownController.RunCountdown(() =>
        {
            Time.timeScale = 1f;
            playerMovement.enabled = true;
        });
    }
}