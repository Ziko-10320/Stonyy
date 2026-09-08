using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [SerializeField] bool destroyStickOnBreak = false;
    [SerializeField] Animator anim;
    const string ANIM_BREAK = "BoxDestruction"; // must match your Trigger parameter name

    Collider2D[] colliders;
    SpriteRenderer sr;
    bool isBroken;

    public bool DestroyStickOnBreak => destroyStickOnBreak;

    void Awake()
    {
        colliders = GetComponents<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

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