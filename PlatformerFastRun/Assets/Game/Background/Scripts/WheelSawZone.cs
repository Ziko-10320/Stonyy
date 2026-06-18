using UnityEngine;

public class WheelSawZone : MonoBehaviour
{
    [SerializeField] Animator wheelSawAnimator;
    [SerializeField] string triggerName;
    [SerializeField] string resetStateName = "Idle"; // the default state to return to on respawn

    bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        wheelSawAnimator.SetTrigger(triggerName);
    }

    public void ResetZone()
    {
        hasTriggered = false;
        wheelSawAnimator.ResetTrigger(triggerName);
        wheelSawAnimator.Play(resetStateName, 0, 0f);
    }
}