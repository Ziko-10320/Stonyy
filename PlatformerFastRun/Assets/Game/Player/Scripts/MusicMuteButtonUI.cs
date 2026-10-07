using UnityEngine;

public class MusicMuteButtonUI : MonoBehaviour
{
    [SerializeField] GameObject musicOnImage;
    [SerializeField] GameObject musicOffImage;

    void Start()
    {
        Refresh();
    }

    // Hook this up to the music mute button's OnClick
    public void OnMusicMuteButtonPressed()
    {
        if (MusicManager.Instance == null) return;
        MusicManager.Instance.ToggleMute();
        Refresh();
    }

    void Refresh()
    {
        bool muted = MusicManager.Instance != null && MusicManager.Instance.IsMuted;
        if (musicOnImage != null) musicOnImage.SetActive(!muted);
        if (musicOffImage != null) musicOffImage.SetActive(muted);
    }
}