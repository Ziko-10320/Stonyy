using UnityEngine;

public class PatrolTriggerZone : MonoBehaviour
{
    [SerializeField] BossHealth boss;
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool triggerOnce = true;

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        boss.StartPatrol();
    }
}