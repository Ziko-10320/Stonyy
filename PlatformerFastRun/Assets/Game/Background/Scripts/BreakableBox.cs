using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [SerializeField] GameObject breakEffect;
    [SerializeField] bool destroyStickOnBreak = false;  // ← add this

    Collider2D[] colliders;
    SpriteRenderer sr;
    bool isBroken;

    public bool DestroyStickOnBreak => destroyStickOnBreak;  // ← add this

    void Awake()
    {
        colliders = GetComponents<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;
        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        foreach (var col in colliders)
            col.enabled = false;
        if (sr != null)
            sr.enabled = false;
    }

    public void ResetBox()
    {
        isBroken = false;
        foreach (var col in colliders)
            col.enabled = true;
        if (sr != null)
            sr.enabled = true;
    }
}