using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardDestroyTrigger : MonoBehaviour
{
    public static readonly List<HazardDestroyTrigger> All = new List<HazardDestroyTrigger>();

    [Header("Hazards")]
    [SerializeField] HazardBoss[] hazards;

    [Header("Timing")]
    [SerializeField] float delayBetweenDisables = 0.5f;

    bool isDisabling;

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }
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