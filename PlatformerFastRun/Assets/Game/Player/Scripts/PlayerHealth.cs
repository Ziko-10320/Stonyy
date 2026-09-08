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
        movement.enabled = false;
        movement.RespawnReset();
        anim.SetBool("Slide", false);
        anim.SetBool("WallSlide", false);
        anim.SetBool("IdleWall", false);
        anim.SetTrigger(ANIM_DEATH);
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPos = checkpointManager != null
            ? checkpointManager.GetLastCheckpointPosition()
            : transform.position;
        transform.position = spawnPos;
        checkpointManager.RespawnBoss();  

        currentHealth = maxHealth;

        foreach (var zone in FindObjectsByType<WheelSawZone>(FindObjectsSortMode.None))
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"WheelSawZone reset failed on {zone.name}: {e}"); }
        }

        foreach (var box in FindObjectsByType<BreakableBox>(FindObjectsSortMode.None))
        {
            try { box.ResetBox(); }
            catch (System.Exception e) { Debug.LogError($"BreakableBox reset failed on {box.name}: {e}"); }
        }

        foreach (var hazard in FindObjectsByType<HazardBoss>(FindObjectsSortMode.None))
        {
            try
            {
                hazard.ResetHazard();
                hazard.gameObject.SetActive(false);
            }
            catch (System.Exception e) { Debug.LogError($"HazardBoss reset failed on {hazard.name}: {e}"); }
        }

        foreach (var zone in FindObjectsByType<HazardSequenceTrigger>(FindObjectsSortMode.None))
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"HazardSequenceTrigger reset failed on {zone.name}: {e}"); }
        }

        foreach (var zone in FindObjectsByType<HazardDestroyTrigger>(FindObjectsSortMode.None))
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"HazardDestroyTrigger reset failed on {zone.name}: {e}"); }
        }
        foreach (var zone in FindObjectsByType<PatrolTriggerZone>(FindObjectsSortMode.None))
        {
            try { zone.ResetTrigger(); }
            catch (System.Exception e) { Debug.LogError($"PatrolTriggerZone reset failed on {zone.name}: {e}"); }
        }
        anim.SetTrigger(ANIM_RESPAWN);
        yield return new WaitForSeconds(0.5f);

        movement.enabled = true;
        movement.RespawnReset();
        movement.ResetDirection();

        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }
    public void InstantKill()
    {
        if (isDead) return;
        currentHealth = 0;
        Die();
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