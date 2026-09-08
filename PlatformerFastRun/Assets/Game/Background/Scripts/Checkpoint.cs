using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    [SerializeField] int checkpointIndex = 0;
    [SerializeField] int bossPhase = 0; // 0 = Phase1, 1 = Phase2, 2 = Phase3 — match BossHealth.Phase order
    public int Index => checkpointIndex;
    public int BossPhase => bossPhase;
    public Vector3 Position => transform.position;
    bool activated;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;
        CheckpointManager manager = FindFirstObjectByType<CheckpointManager>();
        if (manager != null)
            manager.RegisterCheckpoint(this);
        activated = true;
    }
}