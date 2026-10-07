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
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        PlayerPrefs.SetInt("AudioMuted", IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static AudioMuteManager GetOrCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("AudioMuteManager");
            go.AddComponent<AudioMuteManager>();
        }
        return Instance;
    }
}