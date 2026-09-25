using System.Collections.Generic;
using UnityEngine;

public class WheelSawZone : MonoBehaviour
{
    public static readonly List<WheelSawZone> All = new List<WheelSawZone>();

    [SerializeField] Animator wheelSawAnimator;
    [SerializeField] string triggerName;
    [SerializeField] string resetStateName = "Idle"; // the default state to return to on respawn

    bool hasTriggered = false;

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        wheelSawAnimator.SetTrigger(triggerName);
    }

    public void ResetZone()
    {
        hasTriggered = false;
        if (wheelSawAnimator != null)
            wheelSawAnimator.ResetTrigger(triggerName);

        wheelSawAnimator.Play(resetStateName, 0, 0f);
    }
}