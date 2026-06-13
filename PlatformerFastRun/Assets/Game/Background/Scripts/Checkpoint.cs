using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] int checkpointIndex = 0; // set ascending: 0, 1, 2...

    public int Index => checkpointIndex;
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
        // hook animator/particle here if you want a visual pop
    }
}