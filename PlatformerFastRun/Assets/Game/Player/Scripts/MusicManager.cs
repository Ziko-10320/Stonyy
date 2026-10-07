using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] AudioSource musicSource; // AudioSource with your music clip, Loop checked
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

        IsMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        ApplyMute();

        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        PlayerPrefs.SetInt("MusicMuted", IsMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    void ApplyMute()
    {
        if (musicSource != null)
            musicSource.mute = IsMuted;
    }
}