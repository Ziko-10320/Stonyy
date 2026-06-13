using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 3;
    [SerializeField] float invincibilityDuration = 1.5f;

    int currentHealth;
    float invincibilityTimer;
    bool isInvincible;
    bool isDead;

    PlayerMovement movement;
    CheckpointManager checkpointManager;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    const string ANIM_DEATH = "Death";
    const string ANIM_RESPAWN = "Respawn";

    Animator anim;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        checkpointManager = FindFirstObjectByType<CheckpointManager>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                isInvincible = false;
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (isInvincible || isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            // flash feedback — optional, hook your animator here
        }
    }

    void Die()
    {
        isDead = true;
        StartCoroutine(DeathAndRespawnSequence());
    }
    void DestroyAllThrownSticks()
    {
        foreach (GameObject stick in GameObject.FindGameObjectsWithTag("ThrownStick"))
            Destroy(stick);
    }
    IEnumerator DeathAndRespawnSequence()
    {
        DestroyAllThrownSticks();
        // Disable movement immediately
        movement.enabled = false;
        movement.RespawnReset(); // zero velocity / reset state before disabling

        // Play death anim and wait
        anim.SetTrigger(ANIM_DEATH);
        yield return new WaitForSeconds(1f);

        // Teleport to checkpoint
        Vector3 spawnPos = checkpointManager != null
            ? checkpointManager.GetLastCheckpointPosition()
            : transform.position;
        transform.position = spawnPos;

        // Reset health and facing direction
        currentHealth = maxHealth;

        // Play respawn anim and wait
        anim.SetTrigger(ANIM_RESPAWN);
        yield return new WaitForSeconds(1f);

        // Re-enable movement — always facing right
        movement.enabled = true;
        movement.RespawnReset();
        movement.ResetDirection();

        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    void Respawn()
    {
        Vector3 spawnPos = checkpointManager != null
            ? checkpointManager.GetLastCheckpointPosition()
            : transform.position;

        transform.position = spawnPos;
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        movement.RespawnReset();
    }
}