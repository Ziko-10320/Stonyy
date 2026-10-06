using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 3;
    [SerializeField] float invincibilityDuration = 1.5f;
    [Header("Lives")]
    [SerializeField] int maxLives = 10;
    int currentLives;
    PlayerSFX sfx;
    public int CurrentLives => currentLives;
    int currentHealth;
    float invincibilityTimer;
    bool isInvincible;
    bool isDead;
    [Header("Lives UI")]
    [SerializeField] TextMeshProUGUI livesText;
    [Header("Death Panel")]
    [SerializeField] GameObject deathPanel;
    PlayerMovement movement;
    CheckpointManager checkpointManager;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    static readonly int ANIM_DEATH = Animator.StringToHash("Death");
    static readonly int ANIM_RESPAWN = Animator.StringToHash("Respawn");

    Animator anim;

    
    void Awake()
    {
        sfx = GetComponent<PlayerSFX>();
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        checkpointManager = FindFirstObjectByType<CheckpointManager>();
        currentHealth = maxHealth;
        currentLives = maxLives;
        UpdateLivesUI();
    }
    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "" + currentLives;
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
    public void RegenerateLives()
    {
        currentLives = maxLives;
        UpdateLivesUI();
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
    IEnumerator LevelRestartSequence()
    {
        DestroyAllThrownSticks();
        movement.enabled = false;
        movement.RespawnReset();
        anim.SetBool("Slide", false);
        anim.SetBool("WallSlide", false);
        anim.SetBool("IdleWall", false);
        anim.SetTrigger(ANIM_DEATH);
        yield return new WaitForSeconds(0.5f);

        if (deathPanel != null)
            deathPanel.SetActive(true);
    }
    void Die()
    {
        isDead = true;
        sfx?.PlayRandomDeathSound();
        currentLives--;
        UpdateLivesUI();
        if (currentLives <= 0)
            StartCoroutine(LevelRestartSequence());
        else
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

        // Use ToArray() snapshot so ResetZone/SetActive changes to the list mid-loop don't break iteration
        foreach (var zone in WheelSawZone.All.ToArray())
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"WheelSawZone reset failed on {zone.name}: {e}"); }
        }

        foreach (var box in BreakableBox.All.ToArray())
        {
            try { box.ResetBox(); }
            catch (System.Exception e) { Debug.LogError($"BreakableBox reset failed on {box.name}: {e}"); }
        }

        foreach (var hazard in HazardBoss.All.ToArray())
        {
            try
            {
                hazard.ResetHazard();
                hazard.gameObject.SetActive(false);
            }
            catch (System.Exception e) { Debug.LogError($"HazardBoss reset failed on {hazard.name}: {e}"); }
        }

        foreach (var zone in HazardSequenceTrigger.All.ToArray())
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"HazardSequenceTrigger reset failed on {zone.name}: {e}"); }
        }

        foreach (var zone in HazardDestroyTrigger.All.ToArray())
        {
            try { zone.ResetZone(); }
            catch (System.Exception e) { Debug.LogError($"HazardDestroyTrigger reset failed on {zone.name}: {e}"); }
        }

        foreach (var zone in PatrolTriggerZone.All.ToArray())
        {
            try { zone.ResetTrigger(); }
            catch (System.Exception e) { Debug.LogError($"PatrolTriggerZone reset failed on {zone.name}: {e}"); }
        }

        anim.SetTrigger(ANIM_RESPAWN);
        yield return new WaitForSeconds(0.5f);

        movement.enabled = true;
        movement.RespawnReset();
        movement.SetFacingDirection(checkpointManager != null && checkpointManager.GetLastCheckpointFaceLeft());

        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }
    public void OnReplayButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuButtonPressed()
    {
        SceneManager.LoadScene(0);
    }
    public void InstantKill()
    {
        if (isDead) return;
        currentHealth = 0;
        Die();
    }
     
}