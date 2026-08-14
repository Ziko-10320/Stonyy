using System.Collections;
using UnityEngine;

public class HazardBoss : MonoBehaviour
{
    [Header("Warning")]
    [SerializeField] float colliderDisabledDuration = 0.3f;
    [SerializeField] float warningFlashInterval = 0.15f;
    [SerializeField] int warningFlashCount = 4;
    [SerializeField] float fadeInDuration = 0.3f;
    [SerializeField] float warningAlpha = 0.3f;

    SpriteRenderer sr;
    Collider2D col;

    static readonly Color White = Color.white;
    static readonly Color Red = new Color(1f, 0.15f, 0.15f, 1f);

   

    public void Activate()
    {
        StopAllCoroutines();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent< Collider2D>();

        SetAlpha(0f);
        col.enabled = false;

        StartCoroutine(WarningRoutine());
    }
    public void ResetHazard()
    {
        StopAllCoroutines();

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();

        SetColor(White, 0f);
        col.enabled = false;
    }
    IEnumerator WarningRoutine()
    {
        col.enabled = false;

        // Start dim
        SetColor(White, warningAlpha);

        // Flash red ↔ white
        for (int i = 0; i < warningFlashCount; i++)
        {
            // → Red
            float t = 0f;
            while (t < warningFlashInterval)
            {
                t += Time.deltaTime;
                sr.color = Color.Lerp(White, Red, t / warningFlashInterval);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, warningAlpha);
                yield return null;
            }

            // → White
            t = 0f;
            while (t < warningFlashInterval)
            {
                t += Time.deltaTime;
                sr.color = Color.Lerp(Red, White, t / warningFlashInterval);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, warningAlpha);
                yield return null;
            }
        }

        // Make sure we finish on white before fade-in
        SetColor(White, warningAlpha);

        // Fade alpha from warningAlpha → 1
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(warningAlpha, 1f, elapsed / fadeInDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Fully opaque, fully white, collider on
        SetColor(White, 1f);
        col.enabled = true;
    }

    void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    void SetColor(Color rgb, float a)
    {
        sr.color = new Color(rgb.r, rgb.g, rgb.b, a);
    }
}