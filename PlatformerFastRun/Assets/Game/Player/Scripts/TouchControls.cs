using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchControls : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;

    [Header("Enable/Disable")]
    [SerializeField] bool touchControlsEnabled = true;

    int slideTouchId = -1; // left half
    int jumpTouchId = -1;  // right half

    void OnEnable()
    {
        if (!touchControlsEnabled) return;

        EnhancedTouchSupport.Enable();
#if UNITY_EDITOR || UNITY_STANDALONE
        TouchSimulation.Enable();
#endif
    }

    void OnDisable()
    {
        if (!touchControlsEnabled) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        TouchSimulation.Disable();
#endif
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (!touchControlsEnabled) return;

        float halfScreen = Screen.width / 2f;

        foreach (var touch in Touch.activeTouches)
        {
            bool isLeftHalf = touch.screenPosition.x < halfScreen;

            if (touch.phase == TouchPhase.Began)
            {
                if (isLeftHalf && slideTouchId == -1)
                {
                    slideTouchId = touch.touchId;
                    playerMovement.TouchSlidePressed();
                }
                else if (!isLeftHalf && jumpTouchId == -1)
                {
                    jumpTouchId = touch.touchId;
                    playerMovement.TouchJumpPressed();
                }
            }
        }

        if (slideTouchId != -1 && !IsTouchStillActive(slideTouchId))
        {
            slideTouchId = -1;
            playerMovement.TouchSlideReleased();
        }
        if (jumpTouchId != -1 && !IsTouchStillActive(jumpTouchId))
        {
            jumpTouchId = -1;
            playerMovement.TouchJumpReleased();
        }
    }

    bool IsTouchStillActive(int id)
    {
        foreach (var t in Touch.activeTouches)
            if (t.touchId == id) return true;
        return false;
    }
}