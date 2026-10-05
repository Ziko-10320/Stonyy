using UnityEngine;

public class LevelStartCountdown : MonoBehaviour
{
    [SerializeField] CountdownController countdownController;
    [SerializeField] PlayerMovement playerMovement;

    void Start()
    {
        playerMovement.enabled = false;
        countdownController.RunCountdown(() => playerMovement.enabled = true);
    }
}