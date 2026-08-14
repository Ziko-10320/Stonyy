using FirstGearGames.SmoothCameraShaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PatrolPoint
{
    public Transform point;
    public float moveDuration = 1f;   // time to travel TO this point. Set 0 to use defaultMoveSpeed instead
    public float waitDuration = 0f;   // time to freeze once arrived. Set 0 for no pause

    [Header("Animation (set only ONE true)")]
    public bool animRight;
    public bool animLeft;
    public bool animDown;

    [Header("Special Flags")]
    public bool resetToIdle;        // if true, resets boss animator to IdleBoss state
    public bool unlockPhase3Part2;
}

public class BossHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] GameObject[] lifeObjects;

    public ShakeData CameraShakeDeath;

    [Header("Phase 2 Transition")]
    [SerializeField] float delayBeforePhase2 = 2f;
    [SerializeField] GameObject[] phase2Hazards;
    [SerializeField] float riseTargetY = 5f;
    [SerializeField] float riseDuration = 1.5f;
    [SerializeField] float delayBetweenRises = 0.3f;

    [Header("Boss Movement On Hit")]
    [SerializeField] float delayBeforeMove = 1.5f;
    [SerializeField] Transform moveTarget;           // assign the target point in scene
    [SerializeField] Transform moveTargetPhase3;
    [SerializeField] float moveDuration = 1f;

    int livesRemaining;
    bool phase2Triggered;
    Vector3 initialPosition;
    public int LivesRemaining => livesRemaining;
    public int MaxLives => lifeObjects.Length;
    Dictionary<GameObject, Vector3> phase2HazardOrigins = new Dictionary<GameObject, Vector3>();

    [Header("Phase 3 Patrol")]
    [SerializeField] PatrolPoint[] patrolPoints;
    [SerializeField] float defaultMoveSpeed = 5f; // fallback if a point's moveDuration is 0
    [SerializeField] bool loopPatrol = true;
    [SerializeField] Animator animator;

    const string AnimRight = "MoveRight";
    const string AnimLeft = "MoveLeft";
    const string AnimDown = "MoveDown";
    const string AnimIdle = "IdleBoss";

    public bool Phase3Part2Unlocked { get; private set; }
    bool patrolActive;
    Coroutine patrolCoroutine;
    void Awake()
    {
        livesRemaining = lifeObjects.Length;
        initialPosition = transform.position;

        foreach (GameObject hazard in phase2Hazards)
            if (hazard != null)
                phase2HazardOrigins[hazard] = hazard.transform.localPosition;
    }
    public void ResetBoss()
    {
        StopAllCoroutines();

        // reset position
        transform.position = initialPosition;

        // reset lives
        livesRemaining = lifeObjects.Length;
        phase2Triggered = false;
        Phase3Part2Unlocked = false;
        patrolActive = false;

        // re-enable all life objects
        foreach (GameObject life in lifeObjects)
            if (life != null)
                life.SetActive(true);

        // disable phase 2 hazards and reset their local position
        foreach (GameObject hazard in phase2Hazards)
        {
            if (hazard != null)
            {
                hazard.SetActive(false);
                hazard.transform.localPosition = phase2HazardOrigins[hazard];
            }
        }
    }
    public void TakeHit(GameObject hitLifeObject)
    {
        for (int i = 0; i < lifeObjects.Length; i++)
        {
            if (lifeObjects[i] == hitLifeObject && lifeObjects[i].activeSelf)
            {
                livesRemaining--;

                BossLifeObject lifeObj = lifeObjects[i].GetComponent<BossLifeObject>();
                if (lifeObj != null) lifeObj.PlayDestroyEffect();

                CameraShakerHandler.Shake(CameraShakeDeath);
                break;
            }
        }

        StartCoroutine(MoveOnHit(livesRemaining == 1 ? moveTargetPhase3 : moveTarget));

        if (!phase2Triggered)
        {
            phase2Triggered = true;
            StartCoroutine(Phase2Transition());
        }
    }

    IEnumerator MoveOnHit(Transform target)
    {
        yield return new WaitForSeconds(delayBeforeMove);

        if (target == null) yield break;

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
    }

    IEnumerator Phase2Transition()
    {
        yield return new WaitForSeconds(delayBeforePhase2);

        foreach (GameObject hazard in phase2Hazards)
        {
            if (hazard != null)
            {
                hazard.SetActive(true);
                StartCoroutine(RiseUp(hazard));
                yield return new WaitForSeconds(delayBetweenRises);
            }
        }
    }

    IEnumerator RiseUp(GameObject hazard)
    {
        float elapsed = 0f;
        float startY = hazard.transform.localPosition.y;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = hazard.transform.localPosition;
            pos.y = Mathf.Lerp(startY, riseTargetY, smoothT);
            hazard.transform.localPosition = pos;
            yield return null;
        }

        Vector3 final = hazard.transform.localPosition;
        final.y = riseTargetY;
        hazard.transform.localPosition = final;
    }

    public void StartPatrol()
    {
        if (patrolActive || patrolPoints == null || patrolPoints.Length == 0) return;
        patrolActive = true;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    public void StopPatrol()
    {
        patrolActive = false;
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
    }

    IEnumerator PatrolRoutine()
    {
        int index = 0;
        while (patrolActive)
        {
            PatrolPoint pp = patrolPoints[index];
            if (pp.point != null)
            {
                SetPatrolAnimation(pp);
                yield return StartCoroutine(MoveToPosition(pp.point.position, pp.moveDuration));
                if (pp.waitDuration > 0f)
                    yield return new WaitForSeconds(pp.waitDuration);
            }

            index++;
            if (index >= patrolPoints.Length)
            {
                if (!loopPatrol) { patrolActive = false; yield break; }
                index = 0;
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        float dist = Vector3.Distance(startPos, target);
        float dur = duration > 0f ? duration : (defaultMoveSpeed > 0f ? dist / defaultMoveSpeed : 1f);

        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / dur));
            transform.position = Vector3.Lerp(startPos, target, t);
            yield return null;
        }
        transform.position = target;
    }

    void SetPatrolAnimation(PatrolPoint pp)
    {
        if (pp.resetToIdle && animator != null)
        {
            animator.SetTrigger(AnimIdle);
        }
        else if (animator != null)
        {
            if (pp.animRight) animator.SetTrigger(AnimRight);
            if (pp.animLeft) animator.SetTrigger(AnimLeft);
            if (pp.animDown) animator.SetTrigger(AnimDown);
        }

        if (pp.unlockPhase3Part2)
        {
            Phase3Part2Unlocked = true;
        }
    }
}