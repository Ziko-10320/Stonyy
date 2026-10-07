using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    public static readonly List<BreakableBox> All = new List<BreakableBox>();
    [SerializeField] AudioSource audioSource; // plain AudioSource on this box, Play On Awake unchecked
    [SerializeField] AudioClip breakSound;
    [SerializeField] bool destroyStickOnBreak = false;
    [SerializeField] Animator anim;
    const string ANIM_BREAK = "BoxDestruction"; // must match your Trigger parameter name

    Collider2D[] colliders;
    SpriteRenderer sr;
    bool isBroken;

    public bool DestroyStickOnBreak => destroyStickOnBreak;

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }
    void Awake()
    {
        colliders = GetComponents<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void Break()
    {
        if (audioSource != null && breakSound != null && !AudioMuteManager.GetOrCreate().IsMuted)
            audioSource.PlayOneShot(breakSound);
        if (isBroken) return;
        isBroken = true;

        if (audioSource != null && breakSound != null)
            audioSource.PlayOneShot(breakSound); // add this

        if (anim != null)
            anim.SetTrigger(ANIM_BREAK);

        foreach (var col in colliders)
            col.enabled = false;
    }

    public void ResetBox()
    {
        isBroken = false;
        foreach (var col in colliders)
            col.enabled = true;
        if (sr != null)
            sr.enabled = true;
        if (anim != null)
            anim.Play("NothingBox", 0, 0f); // replace "Idle" with your actual default/idle state name
    }
}