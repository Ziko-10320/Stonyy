using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] GameObject killWallPrefab; // simple GameObject: BoxCollider2D (Is Trigger) + BoundaryKill script
    [SerializeField] float wallOffsetX = -1.5f; // how far behind the checkpoint to place it (negative = behind if moving right)
    [SerializeField] GameObject[] killWalls;
    GameObject currentWallInstance;
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
        if (lastCheckpoint == null || checkpoint.Index > lastCheckpoint.Index)
        {
            // disable the previous checkpoint's wall
            if (lastCheckpoint != null && lastCheckpoint.Index < killWalls.Length && killWalls[lastCheckpoint.Index] != null)
                killWalls[lastCheckpoint.Index].SetActive(false);

            lastCheckpoint = checkpoint;

            // enable this checkpoint's wall
            if (checkpoint.Index < killWalls.Length && killWalls[checkpoint.Index] != null)
                killWalls[checkpoint.Index].SetActive(true);
        }
    }

    void SpawnKillWall(Vector3 checkpointPos)
    {
        Vector3 wallPos = checkpointPos + new Vector3(wallOffsetX, 0f, 0f);

        if (currentWallInstance == null)
            currentWallInstance = Instantiate(killWallPrefab, wallPos, Quaternion.identity);
        else
            currentWallInstance.transform.position = wallPos;
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpoint != null ? lastCheckpoint.Position : defaultSpawnPosition;
    }
}