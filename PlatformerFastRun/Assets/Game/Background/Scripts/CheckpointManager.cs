using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    Checkpoint lastCheckpoint;
    Vector3 defaultSpawnPosition; // fallback if no checkpoint hit yet

    void Awake()
    {
        // Find the player start position as fallback
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            defaultSpawnPosition = player.transform.position;
    }

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        // Only update if this checkpoint is further than the current one
        if (lastCheckpoint == null || checkpoint.Index > lastCheckpoint.Index)
            lastCheckpoint = checkpoint;
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpoint != null ? lastCheckpoint.Position : defaultSpawnPosition;
    }
}