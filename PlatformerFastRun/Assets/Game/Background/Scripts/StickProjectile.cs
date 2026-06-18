using UnityEngine;

public class StickProjectile : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    bool isStuck;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // Only solid ground stops the stick
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
            StickInPlace();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Boxes are detected via trigger only — no physics response, stick keeps flying
        BreakableBox box = other.GetComponent<BreakableBox>();
        if (box != null && !isStuck)
            box.Break();
    }

    void StickInPlace()
    {
        if (isStuck) return;
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }
}