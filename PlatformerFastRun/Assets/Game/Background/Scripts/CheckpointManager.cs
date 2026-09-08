using UnityEngine;
public class CheckpointManager : MonoBehaviour
{
    [SerializeField] GameObject killWallPrefab;
    [SerializeField] float wallOffsetX = -1.5f;
    [SerializeField] GameObject[] killWalls;
    [SerializeField] BossHealth bossHealth; // ? assign in Inspector

    GameObject currentWallInstance;
    Checkpoint lastCheckpoint;
    Vector3 defaultSpawnPosition;
    int lastCheckpointBossPhase = 0; // ? defaults to Phase1

    void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            defaultSpawnPosition = player.transform.position;
    }

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (lastCheckpoint == null || checkpoint.Index > lastCheckpoint.Index)
        {
            if (lastCheckpoint != null && lastCheckpoint.Index < killWalls.Length && killWalls[lastCheckpoint.Index] != null)
                killWalls[lastCheckpoint.Index].SetActive(false);
            lastCheckpoint = checkpoint;
            lastCheckpointBossPhase = checkpoint.BossPhase; // ? record phase alongside position
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

    // ? NEW: call this wherever your existing respawn logic already moves the player
    public void RespawnBoss()
    {
        if (bossHealth != null)
            bossHealth.RestoreToPhase(lastCheckpointBossPhase);
    }
}