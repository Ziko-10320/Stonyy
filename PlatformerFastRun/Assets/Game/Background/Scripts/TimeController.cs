using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0f, 3f)]
    [Tooltip("1 is normal speed. 0.5 is half speed. 0 is paused.")]
    public float globalTimeScale = 1.0f;

    void Update()
    {
        // Only apply changes if the slider value doesn't match the actual time scale
        if (Time.timeScale != globalTimeScale)
        {
            Time.timeScale = globalTimeScale;

            // THE PRO TRICK: You MUST adjust fixedDeltaTime when you change timeScale.
            // If you don't, your 2D physics (jumping, falling) will become extremely jittery in slow-mo.
            // 0.02f is Unity's default fixedDeltaTime.
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    // Safety net: If you delete this object or change scenes, time goes back to normal.
    void OnDisable()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}
