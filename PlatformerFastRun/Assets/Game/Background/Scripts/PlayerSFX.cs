using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [Header("Loop Sounds")]
    [SerializeField] AudioSource runLoopSource;   // AudioSource with the run clip, Loop checked, Play On Awake unchecked
    [SerializeField] AudioSource slideLoopSource; // same setup, slide clip

    [Header("One-Shot Source")]
    [SerializeField] AudioSource sfxSource; // plain AudioSource, Play On Awake unchecked, used for everything below

    [Header("Death Sounds")]
    [SerializeField] AudioClip[] deathClips; // drag both death clips in here

    [Header("Stick Throw")]
    [SerializeField] AudioClip stickThrowClip;

    // Hook this up as the animation event's function — jump, land, dash, upward dash
    // each event just passes in whichever clip it needs as the Object parameter
    public void PlaySound(AudioClip clip)
    {
        if (clip == null || AudioMuteManager.GetOrCreate().IsMuted) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayRandomDeathSound()
    {
        if (deathClips == null || deathClips.Length == 0 || AudioMuteManager.GetOrCreate().IsMuted) return;
        AudioClip clip = deathClips[Random.Range(0, deathClips.Length)];
        sfxSource.PlayOneShot(clip);
    }

    public void PlayStickThrowSound()
    {
        if (stickThrowClip == null || AudioMuteManager.GetOrCreate().IsMuted) return;
        sfxSource.PlayOneShot(stickThrowClip);
    }

    public void SetRunLoop(bool shouldPlay)
    {
        if (runLoopSource == null) return;
        bool play = shouldPlay && !AudioMuteManager.GetOrCreate().IsMuted;
        if (play && !runLoopSource.isPlaying) runLoopSource.Play();
        else if (!play && runLoopSource.isPlaying) runLoopSource.Stop();
    }

    public void SetSlideLoop(bool shouldPlay)
    {
        if (slideLoopSource == null) return;
        bool play = shouldPlay && !AudioMuteManager.GetOrCreate().IsMuted;
        if (play && !slideLoopSource.isPlaying) slideLoopSource.Play();
        else if (!play && slideLoopSource.isPlaying) slideLoopSource.Stop();
    }
}