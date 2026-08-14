using System.Collections;
using UnityEngine;

public class HazardSequenceTrigger : MonoBehaviour
{
    [Header("Hazards")]
    [SerializeField] HazardBoss[] hazards;

    [Header("Timing")]
    [SerializeField] float delayBetweenHazards = 1f;

    [Header("Phase Requirement")]
    [SerializeField] BossHealth bossHealth;
    [SerializeField] bool requirePhase1;
    [SerializeField] bool requirePhase2;
    [SerializeField] bool requirePhase3;
    [SerializeField] bool requirePhase3Part2;

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        TryActivate(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryActivate(other);
    }

    void TryActivate(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (!IsRequiredPhase()) return;

        triggered = true;
        StartCoroutine(ActivateSequence());
    }

    bool IsRequiredPhase()
    {
        if (bossHealth == null) return true; // no boss assigned, always trigger

        int lives = bossHealth.LivesRemaining;
        int max = bossHealth.MaxLives;

        if (requirePhase1 && lives == max) return true;
        if (requirePhase2 && lives == max - 1) return true;
        if (requirePhase3 && lives == max - 2) return true;
        if (requirePhase3Part2 && bossHealth.Phase3Part2Unlocked) return true;
        return false;
    }

    public void ResetZone()
    {
        triggered = false;
    }

    IEnumerator ActivateSequence()
    {
        foreach (HazardBoss hazard in hazards)
        {
            hazard.gameObject.SetActive(true);
            hazard.Activate();
            yield return new WaitForSeconds(delayBetweenHazards);
        }
    }
}