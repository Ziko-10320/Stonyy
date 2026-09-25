using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] int fontSize = 60;
    [SerializeField] Color textColor = Color.yellow;

    float deltaTime;
    GUIStyle style;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // keep showing across every scene
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
        }

        float fps = 1.0f / deltaTime;
        string text = $"{fps:0.} FPS\n" +
                      $"Target: {Application.targetFrameRate}\n" +
                      $"VSync: {QualitySettings.vSyncCount}\n" +
                      $"Refresh: {Screen.currentResolution.refreshRateRatio.value:0.0}Hz\n" +
                      $"Res: {Screen.width}x{Screen.height}";

        GUI.Label(new Rect(20, 20, 400, 250), text, style);
    }
}