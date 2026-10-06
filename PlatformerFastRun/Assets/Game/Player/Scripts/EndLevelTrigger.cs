using System.Collections;
using UnityEngine;

public class EndLevelTrigger : MonoBehaviour
{
    [Header("Friend")]
    [SerializeField] Animator friendAnimator;

    [Header("Animator Trigger Names")]
    [SerializeField] string playerEndLevelTrigger = "EndLevel";
    [SerializeField] string friendEndLevelTrigger = "FriendEndLevel";

    [Header("Camera Zoom")]
    [SerializeField] Camera targetCamera;           // drag Main Camera here (or leave empty to auto-use Camera.main)
    [SerializeField] Transform zoomFocusPoint;      // the point to zoom into
    [SerializeField] float zoomedOrthoSize = 3f;    // smaller = more zoomed in (orthographic camera)
    [SerializeField] float zoomDuration = 1.2f;
    [SerializeField] AnimationCurve zoomEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] bool followFocusPointPosition = true; // move camera toward the point, or just zoom in place

    [Header("Optional: disable camera follow script during cutscene")]
    [SerializeField] MonoBehaviour cameraFollowScriptToDisable;
    [Header("Level Progress")]
    [SerializeField] string thisLevelId;
    bool triggered;

    [Header("End Menu")]
    [SerializeField] bool showEndMenu = true;
    [SerializeField] EndLevelMenu endLevelMenu;
    [SerializeField] float delayBeforeMenu = 2f; // time for end-level animations to play out

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return; // make sure your player GameObject is tagged "Player"

        triggered = true;
        LevelProgress.SetCompleted(thisLevelId);
        var movement = other.GetComponent<PlayerMovement>();
        var health = other.GetComponent<PlayerHealth>();
        var anim = other.GetComponent<Animator>();
        var sfx = other.GetComponent<PlayerSFX>();
        if (movement != null)
        {
            movement.StopForCutscene(); // zero velocity/gravity, clear anim flags
            movement.enabled = false;
            sfx?.SetRunLoop(false);
            sfx?.SetSlideLoop(false);// stops Update/FixedUpdate + input entirely
        }

        if (health != null)
            health.enabled = false;     // stops damage/invincibility ticking

        if (anim != null)
            anim.SetTrigger(playerEndLevelTrigger);

        if (friendAnimator != null)
            friendAnimator.SetTrigger(friendEndLevelTrigger);

        if (cameraFollowScriptToDisable != null)
            cameraFollowScriptToDisable.enabled = false;

        StartCoroutine(ZoomCamera());
        StartCoroutine(ShowMenuAfterDelay());
    }

    IEnumerator ZoomCamera()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || zoomFocusPoint == null) yield break;

        float startSize = cam.orthographicSize;
        Vector3 startPos = cam.transform.position;
        Vector3 endPos = followFocusPointPosition
            ? new Vector3(zoomFocusPoint.position.x, zoomFocusPoint.position.y, startPos.z)
            : startPos;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float eval = zoomEase.Evaluate(Mathf.Clamp01(t / zoomDuration));

            cam.orthographicSize = Mathf.Lerp(startSize, zoomedOrthoSize, eval);
            cam.transform.position = Vector3.Lerp(startPos, endPos, eval);

            yield return null;
        }

        cam.orthographicSize = zoomedOrthoSize;
        cam.transform.position = endPos;
    }
    IEnumerator ShowMenuAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeMenu);
        if (endLevelMenu == null) yield break;

        if (showEndMenu)
            endLevelMenu.ShowMenu();
        else
            endLevelMenu.GoToMainMenuDirectly();
    }
}