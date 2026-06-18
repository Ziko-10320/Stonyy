using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] float collapseDelay = 1f;
    [SerializeField] float disappearDelay = 2f;

    [Header("Fade")]
    [SerializeField] bool fadeOut = true;
    [SerializeField] float fadeDuration = 0.4f;

    Collider2D col;
    SpriteRenderer sr;
    Color originalColor;
    bool triggered;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (triggered) return;
        if (!other.gameObject.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(collapseDelay);

        // Fade out sprite
        if (fadeOut && sr != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
                yield return null;
            }
        }

        // Disable collider so player falls through
        col.enabled = false;

        // Wait then hide sprite — never disable the GameObject
        yield return new WaitForSeconds(disappearDelay);
        if (sr != null) sr.enabled = false;
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        triggered = false;
        col.enabled = true;
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = originalColor;
        }
    }
}