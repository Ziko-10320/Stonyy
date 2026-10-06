using UnityEngine;

public class AudioMuteManager : MonoBehaviour
{
    public static AudioMuteManager Instance;
    public bool IsMuted { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
        ApplyMute();
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        PlayerPrefs.SetInt("AudioMuted", IsMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    void ApplyMute()
    {
        AudioListener.volume = IsMuted ? 0f : 1f;
    }
}