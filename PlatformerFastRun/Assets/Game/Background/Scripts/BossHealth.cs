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


    [Header("Phase Life Configuration")]
    [SerializeField] GameObject[] phase1ActiveLives; // exact life objects that should be ON when restoring to Phase1
    [SerializeField] GameObject[] phase2ActiveLives; // e.g. 2 specific life objects for Phase2
    [SerializeField] GameObject[] phase3ActiveLives; // e.g. 1 specific life object for Phase3

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

    [Header("Eye Tracking - Outer")]
    [SerializeField] Transform eyeTransform;
    [SerializeField] Transform eyeSocketCenter;
    [SerializeField] float eyeLookRadius = 0.3f;
    [SerializeField] float eyeTrackSpeed = 10f;

    [Header("Eye Tracking - Inner (pupil/iris)")]
    [SerializeField] Transform innerEyeTransform;
    [SerializeField] Transform innerEyeSocketCenter;   // usually same spot as outer, just a separate ref for flexibility
    [SerializeField] float innerEyeLookRadius = 0.12f; // smaller radius than outer eye
    [SerializeField] float innerEyeTrackSpeed = 14f;   // often a bit snappier than the outer eye

    [Header("Shared")]
    [SerializeField] Transform player;

    Vector3 eyeLocalOrigin;
    Vector3 innerEyeLocalOrigin;

    [Header("Random Reactions - Phase 1")]
    [SerializeField] string phase1Reaction1Anim = "RandomReaction1_P1";
    [SerializeField] string phase1Reaction2Anim = "RandomReaction2_P1";
    [SerializeField, Range(0f, 1f)] float phase1Reaction1Chance = 0.3f;
    [SerializeField, Range(0f, 1f)] float phase1Reaction2Chance = 0.3f;

    [Header("Random Reactions - Phase 2")]
    [SerializeField] string phase2Reaction1Anim = "RandomReaction1_P2";
    [SerializeField] string phase2Reaction2Anim = "RandomReaction2_P2";
    [SerializeField, Range(0f, 1f)] float phase2Reaction1Chance = 0.3f;
    [SerializeField, Range(0f, 1f)] float phase2Reaction2Chance = 0.3f;

    [Header("Random Reaction Settings")]
    [SerializeField] float reactionCheckInterval = 2f;   // how often to roll the dice
    [SerializeField] float eyeResetDuration = 0.75f;      // 0.5–1s eye recenter time

    bool eyeTrackingEnabled = true;

    public enum Phase { Phase1, Phase2, Phase3 } 
    Phase currentPhase = Phase.Phase1;
    Coroutine reactionWatcherCoroutine;

    string lastReactionPlayed = "";

    [Header("Reaction Visibility Gate")]
    [SerializeField] Transform rightCheckPoint;
    [SerializeField] Transform leftCheckPoint;
    [SerializeField] Vector2 rightCheckSize = new Vector2(2f, 2f);
    [SerializeField] Vector2 leftCheckSize = new Vector2(2f, 2f);
    [SerializeField] LayerMask playerLayer;

    bool isReactionPlaying;

    bool patrolTriggerReached;  
    void Awake()
    {
        livesRemaining = lifeObjects.Length;
        initialPosition = transform.position;

        foreach (GameObject hazard in phase2Hazards)
            if (hazard != null)
                phase2HazardOrigins[hazard] = hazard.transform.localPosition;

        if (eyeTransform != null)
            eyeLocalOrigin = eyeTransform.localPosition;

        if (innerEyeTransform != null)
            innerEyeLocalOrigin = innerEyeTransform.localPosition;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        reactionWatcherCoroutine = StartCoroutine(ReactionWatcher());
    }
    void Update()
    {
        if (!eyeTrackingEnabled) return;
        UpdateEyeTracking(eyeTransform, eyeSocketCenter, eyeLocalOrigin, eyeLookRadius, eyeTrackSpeed);
        UpdateEyeTracking(innerEyeTransform, innerEyeSocketCenter, innerEyeLocalOrigin, innerEyeLookRadius, innerEyeTrackSpeed);
    }

    void UpdateEyeTracking(Transform eye, Transform center, Vector3 localOrigin, float radius, float speed)
    {
        if (eye == null || center == null || player == null) return;

        Vector3 dirToPlayer = player.position - center.position;
        dirToPlayer.z = 0f;

        float distFactor = Mathf.Clamp01(dirToPlayer.magnitude / radius);
        Vector3 targetLocalPos = localOrigin + (Vector3)(dirToPlayer.normalized * radius * distFactor);

        eye.localPosition = Vector3.Lerp(eye.localPosition, targetLocalPos, speed * Time.deltaTime);
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

        currentPhase = Phase.Phase1;
      
        eyeTrackingEnabled = true;

        if (reactionWatcherCoroutine != null) StopCoroutine(reactionWatcherCoroutine);
        reactionWatcherCoroutine = StartCoroutine(ReactionWatcher());

        lastReactionPlayed = "";

        patrolTriggerReached = false;
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
            currentPhase = Phase.Phase2;
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
        patrolTriggerReached = true;
        currentPhase = Phase.Phase3;
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
    bool IsPlayerInHitRange()
    {
        bool rightHit = rightCheckPoint != null &&
            Physics2D.OverlapBox(rightCheckPoint.position, rightCheckSize, 0f, playerLayer);

        bool leftHit = leftCheckPoint != null &&
            Physics2D.OverlapBox(leftCheckPoint.position, leftCheckSize, 0f, playerLayer);

        return rightHit || leftHit;
    }
    IEnumerator ReactionWatcher()
    {
        while (true)
        {
            yield return new WaitForSeconds(reactionCheckInterval);

            if (IsPlayerInHitRange()) continue; // player is close enough to attack — skip this roll

            if (currentPhase == Phase.Phase1)
            {
                yield return StartCoroutine(TryPlayPhaseReaction(
                    phase1Reaction1Anim, phase1Reaction1Chance,
                    phase1Reaction2Anim, phase1Reaction2Chance));
            }
            else if (currentPhase == Phase.Phase2)
            {
                yield return StartCoroutine(TryPlayPhaseReaction(
                    phase2Reaction1Anim, phase2Reaction1Chance,
                    phase2Reaction2Anim, phase2Reaction2Chance));
            }
        }
    }

    IEnumerator TryPlayPhaseReaction(string anim1, float chance1, string anim2, float chance2)
    {
        // Build the list of eligible reactions (excluding whichever played last time)
        bool anim1Eligible = anim1 != lastReactionPlayed;
        bool anim2Eligible = anim2 != lastReactionPlayed;

        bool rollAnim1 = anim1Eligible && Random.value < chance1;
        bool rollAnim2 = anim2Eligible && Random.value < chance2;

        if (rollAnim1)
        {
            yield return StartCoroutine(PlayReaction(anim1));
        }
        else if (rollAnim2)
        {
            yield return StartCoroutine(PlayReaction(anim2));
        }
        // If the only eligible one didn't roll successfully, or the eligible one is excluded, nothing plays this interval — normal, just waits for next check
    }

    IEnumerator PlayReaction(string animName)
    {
        eyeTrackingEnabled = false;
        isReactionPlaying = true;

        float t = 0f;
        Vector3 startOuter = eyeTransform != null ? eyeTransform.localPosition : Vector3.zero;
        Vector3 startInner = innerEyeTransform != null ? innerEyeTransform.localPosition : Vector3.zero;

        while (t < eyeResetDuration)
        {
            if (IsPlayerInHitRange())
            {
                AbortReaction();
                yield break;
            }

            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / eyeResetDuration);
            if (eyeTransform != null)
                eyeTransform.localPosition = Vector3.Lerp(startOuter, eyeLocalOrigin, p);
            if (innerEyeTransform != null)
                innerEyeTransform.localPosition = Vector3.Lerp(startInner, innerEyeLocalOrigin, p);
            yield return null;
        }
        if (eyeTransform != null) eyeTransform.localPosition = eyeLocalOrigin;
        if (innerEyeTransform != null) innerEyeTransform.localPosition = innerEyeLocalOrigin;

        if (IsPlayerInHitRange())
        {
            AbortReaction();
            yield break;
        }

        if (animator != null)
        {
            animator.SetTrigger(animName);
            yield return null;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            float elapsed = 0f;
            while (elapsed < info.length)
            {
                if (IsPlayerInHitRange())
                {
                    AbortReaction();
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        lastReactionPlayed = animName;
        isReactionPlaying = false;
        eyeTrackingEnabled = true;
    }

    void AbortReaction()
    {
        if (animator != null)
            animator.SetTrigger(AnimIdle); // forces back to IdleBoss immediately
        isReactionPlaying = false;
        eyeTrackingEnabled = true;
    }

    public void RestoreToPhase(int phaseIndex)
    {
        StopAllCoroutines();

        Phase targetPhase = (Phase)phaseIndex;
        currentPhase = targetPhase;
        phase2Triggered = targetPhase != Phase.Phase1;
        Phase3Part2Unlocked = false; // always revert to base Phase3 on respawn

        // Position
        if (targetPhase == Phase.Phase1)
            transform.position = initialPosition;
        else if (targetPhase == Phase.Phase2 && moveTarget != null)
            transform.position = moveTarget.position;
        else if (targetPhase == Phase.Phase3 && moveTargetPhase3 != null)
            transform.position = moveTargetPhase3.position;

        // Lives — exact objects per phase
        ApplyLifeConfigForPhase(targetPhase);

        // Hazards — up for Phase2/Phase3, down for Phase1
        bool hazardsUp = targetPhase == Phase.Phase2 || targetPhase == Phase.Phase3;
        foreach (GameObject hazard in phase2Hazards)
        {
            if (hazard == null) continue;
            hazard.SetActive(hazardsUp);
            Vector3 pos = hazard.transform.localPosition;
            pos.y = hazardsUp ? riseTargetY : phase2HazardOrigins[hazard].y;
            hazard.transform.localPosition = pos;
        }

        // Patrol
        patrolActive = false; // always freeze on respawn — must re-enter trigger zone
        patrolTriggerReached = false; // reset so trigger zone logic is consistent

        // Reaction system
        eyeTrackingEnabled = true;
        isReactionPlaying = false;
        lastReactionPlayed = "";
        reactionWatcherCoroutine = StartCoroutine(ReactionWatcher());
    }

    void ApplyLifeConfigForPhase(Phase phase)
    {
        foreach (GameObject life in lifeObjects)
            if (life != null) life.SetActive(false);

        GameObject[] activeSet = phase switch
        {
            Phase.Phase1 => phase1ActiveLives,
            Phase.Phase2 => phase2ActiveLives,
            Phase.Phase3 => phase3ActiveLives,
            _ => phase1ActiveLives
        };

        foreach (GameObject life in activeSet)
            if (life != null) life.SetActive(true);

        livesRemaining = activeSet.Length;
    }
    void OnDrawGizmos()
    {
        if (eyeSocketCenter != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f); // cyan = outer
            DrawGizmoCircle(eyeSocketCenter.position, eyeLookRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(eyeSocketCenter.position, 0.05f);
        }

        if (innerEyeSocketCenter != null)
        {
            Gizmos.color = new Color(1f, 0.4f, 1f, 0.8f); // magenta = inner
            DrawGizmoCircle(innerEyeSocketCenter.position, innerEyeLookRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(innerEyeSocketCenter.position, 0.03f);
        }

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.red;
            if (eyeSocketCenter != null) Gizmos.DrawLine(eyeSocketCenter.position, player.position);
        }

        // Right check box
        if (rightCheckPoint != null)
        {
            bool rightPlayerIn = Application.isPlaying &&
                Physics2D.OverlapBox(rightCheckPoint.position, rightCheckSize, 0f, playerLayer);
            Gizmos.color = rightPlayerIn ? new Color(1f, 0f, 0f, 0.9f) : new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireCube(rightCheckPoint.position, rightCheckSize);
        }

        // Left check box
        if (leftCheckPoint != null)
        {
            bool leftPlayerIn = Application.isPlaying &&
                Physics2D.OverlapBox(leftCheckPoint.position, leftCheckSize, 0f, playerLayer);
            Gizmos.color = leftPlayerIn ? new Color(1f, 0f, 0f, 0.9f) : new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireCube(leftCheckPoint.position, leftCheckSize);
        }
    }
    void DrawGizmoCircle(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}