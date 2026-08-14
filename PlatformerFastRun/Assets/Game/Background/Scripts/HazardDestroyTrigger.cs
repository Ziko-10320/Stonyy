using System.Collections;
using UnityEngine;

public class HazardDestroyTrigger : MonoBehaviour
{
    [Header("Hazards")]
    [SerializeField] HazardBoss[] hazards;

    [Header("Timing")]
    [SerializeField] float delayBetweenDisables = 0.5f;

    bool isDisabling;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDisabling) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(DisableSequence());
    }

    public void ResetZone()
    {
        StopAllCoroutines();
        isDisabling = false;
    }

    IEnumerator DisableSequence()
    {
        isDisabling = true;

        foreach (HazardBoss hazard in hazards)
        {
            if (hazard != null && hazard.gameObject.activeSelf)
            {
                hazard.gameObject.SetActive(false);
                yield return new WaitForSeconds(delayBetweenDisables);
            }
        }

        isDisabling = false;
    }
}