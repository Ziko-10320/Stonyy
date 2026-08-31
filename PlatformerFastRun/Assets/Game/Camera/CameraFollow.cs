using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Range(0.01f, 1.0f)]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 0, -10);
    [Header("Fall Lookahead")]
    [SerializeField] private float fallLookaheadDistance = 3f; // how far down the camera peeks
    [SerializeField] private float fallLookaheadSmoothing = 0.05f; // how gradually it eases in/out
    [SerializeField] private Collider2D[] lookaheadZones;
    [SerializeField] private float fallDetectionThreshold = 0.05f; // ignore Y jitter smaller than this
    [SerializeField] private float fallReleaseDelay = 0.25f; // seconds of non-falling before lookahead turns off

    private float timeSinceLastFall;
    private float lastTargetY;
    private float currentLookaheadY;
    [Header("Axis Locking")]
    [SerializeField] private bool lockYAxis = false;

    [Header("Camera Bounds")]
    [SerializeField] private Collider2D[] cameraBounds; // drag a BoxCollider2D marking the level edges
    [SerializeField] private bool useBounds = true;
    private Dictionary<Collider2D, bool> wallArmed = new Dictionary<Collider2D, bool>();
    private Dictionary<Collider2D, bool> wallWasActive = new Dictionary<Collider2D, bool>();
    private Vector3 currentOffset;
    private Camera cam;

[Range(0.01f, 1.0f)]
[SerializeField] private float boundsSmoothSpeed = 0.08f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            lastTargetY = target.position.y;
    }
    Collider2D GetActiveBounds()
    {
        if (cameraBounds == null || cameraBounds.Length == 0) return null;

        foreach (var b in cameraBounds)
        {
            if (b != null && b.OverlapPoint(target.position))
                return b;
        }
        return null;
    }
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow script has no target assigned!");
            return;
        }

        // --- 2. CALCULATE DESIRED POSITION ---
        Vector3 desiredPosition = target.position + currentOffset;
        float targetYDelta = target.position.y - lastTargetY;
        lastTargetY = target.position.y;

        bool isFallingNow = targetYDelta < -fallDetectionThreshold;

        if (isFallingNow)
            timeSinceLastFall = 0f;
        else
            timeSinceLastFall += Time.deltaTime;

        bool stillCountsAsFalling = timeSinceLastFall < fallReleaseDelay;

        float lookaheadTarget = (stillCountsAsFalling && IsInLookaheadZone()) ? fallLookaheadDistance : 0f;
        currentLookaheadY = Mathf.Lerp(currentLookaheadY, lookaheadTarget, fallLookaheadSmoothing);

        desiredPosition.y -= currentLookaheadY;
        // --- 3. APPLY Y-AXIS LOCK (IF ENABLED) ---
        if (lockYAxis)
        {
            desiredPosition.y = transform.position.y;
        }

        // --- 4. SMOOTHLY MOVE THE CAMERA ---
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // --- 5. CLAMP TO LEVEL BOUNDS (IF ENABLED) ---
        if (useBounds && cam != null)
        {
            Vector3 clampedTarget = ClampToBounds(smoothedPosition);
            smoothedPosition = Vector3.Lerp(transform.position, clampedTarget, boundsSmoothSpeed);
        }

        // --- 6. APPLY THE FINAL POSITION ---
        transform.position = smoothedPosition;
    }
    bool IsInLookaheadZone()
    {
        if (lookaheadZones == null || lookaheadZones.Length == 0) return false;

        foreach (var zone in lookaheadZones)
        {
            if (zone != null && zone.OverlapPoint(target.position))
                return true;
        }
        return false;
    }
    Vector3 ClampToBounds(Vector3 pos)
    {
        if (cameraBounds == null) return pos;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        foreach (var wall in cameraBounds)
        {
            bool isActiveNow = wall.gameObject.activeInHierarchy;
            bool hadEntry = wallWasActive.TryGetValue(wall, out var prevActive);
            bool wasActiveBefore = hadEntry && prevActive;

            if (wall != null)
                Debug.Log($"[{wall.name}] active={wall.gameObject.activeInHierarchy} armed={(wallArmed.TryGetValue(wall, out var a) && a)} bounds={wall.bounds}");
            if (!isActiveNow)
            {
                wallWasActive[wall] = false;
                continue;
            }

            if (!wasActiveBefore)
            {
                // If we've never tracked this wall before, it was already active when the scene started
                // (a permanent CC wall) -> arm it immediately, no waiting.
                // If we HAVE tracked it before as inactive, it just got switched on now during gameplay
                // (a WC wall spawning in) -> require the camera to leave it first before it can push.
                wallArmed[wall] = !hadEntry;
            }

            wallWasActive[wall] = true;

            Bounds b = wall.bounds;
            float camMinX = pos.x - camHalfWidth;
            float camMaxX = pos.x + camHalfWidth;
            float camMinY = pos.y - camHalfHeight;
            float camMaxY = pos.y + camHalfHeight;

            bool overlapX = camMaxX > b.min.x && camMinX < b.max.x;
            bool overlapY = camMaxY > b.min.y && camMinY < b.max.y;

            bool armed = wallArmed.TryGetValue(wall, out var isArmed) && isArmed;

            if (!armed)
            {
                // not armed yet - only becomes armed once camera has fully left the wall
                if (!overlapX || !overlapY)
                    wallArmed[wall] = true;

                continue; // never push while unarmed
            }

            if (!overlapX || !overlapY) continue; // armed but camera isn't touching it right now

            float pushLeft = camMaxX - b.min.x;
            float pushRight = b.max.x - camMinX;
            float pushDown = camMaxY - b.min.y;
            float pushUp = b.max.y - camMinY;

            float minX = Mathf.Min(pushLeft, pushRight);
            float minY = Mathf.Min(pushDown, pushUp);

            if (minX < minY)
                pos.x += (pushLeft < pushRight) ? -pushLeft : pushRight;
            else
                pos.y += (pushDown < pushUp) ? -pushDown : pushUp;
        }

        return pos;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetBounds(Collider2D[] newBounds)
    {
        cameraBounds = newBounds;
    }
}