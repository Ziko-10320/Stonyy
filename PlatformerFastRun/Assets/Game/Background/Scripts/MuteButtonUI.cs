using UnityEngine;

public class MuteButtonUI : MonoBehaviour
{
    [SerializeField] GameObject soundOnImage;
    [SerializeField] GameObject soundOffImage;

    void Start()
    {
        Refresh();
    }

    // Hook this up to the mute button's OnClick
    public void OnMuteButtonPressed()
    {
        AudioMuteManager.Instance.ToggleMute();
        Refresh();
    }

    void Refresh()
    {
        bool muted = AudioMuteManager.Instance != null && AudioMuteManager.Instance.IsMuted;
        if (soundOnImage != null) soundOnImage.SetActive(!muted);
        if (soundOffImage != null) soundOffImage.SetActive(muted);
    }
}