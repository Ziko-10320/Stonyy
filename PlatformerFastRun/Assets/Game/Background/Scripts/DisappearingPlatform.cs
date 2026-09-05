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

    [Header("Effects")]
    [SerializeField] GameObject dustPrefab;
     

    [Header("Fallback Detection")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float detectionPadding = 0.1f;

    Animator anim;
    const string ANIM_COLLAPSE = "Collapse"; // must match your Trigger parameter name in the Animator

    Collider2D col;
    SpriteRenderer sr;
    Color originalColor;
    bool triggered;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (sr != null) originalColor = sr.color;
    }
     

    void FixedUpdate()
    {
        if (triggered) return;

        Vector2 size = col.bounds.size;
        Vector2 center = col.bounds.center + Vector3.up * (size.y / 2f + detectionPadding);

        Collider2D hit = Physics2D.OverlapBox(center, new Vector2(size.x, detectionPadding * 2f), 0f, playerLayer);
        if (hit != null && hit.CompareTag("Player"))
        {
            TriggerCollapse();
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (triggered) return;
        if (!other.gameObject.CompareTag("Player")) return;
        triggered = true;

        if (dustPrefab != null)
            Instantiate(dustPrefab, transform.position, Quaternion.identity);

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(collapseDelay);
        if (anim != null)
            anim.SetTrigger(ANIM_COLLAPSE);
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
    public void TriggerCollapse()
    {
        if (triggered) return;
        triggered = true;
        if (dustPrefab != null)
        {
            var fx = Instantiate(dustPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        StartCoroutine(Sequence());
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
        if (anim != null)
            anim.Play("NothingDisappearing", 0, 0f);
    }
}