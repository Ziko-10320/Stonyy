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
        // hit a breakable box
        BreakableBox box = other.GetComponent<BreakableBox>();
        if (box != null && !isStuck)
        {
            box.Break();
            if (box.DestroyStickOnBreak)
                Destroy(gameObject);
            return;
        }

        // hit a boss life object
        BossLifeObject life = other.GetComponent<BossLifeObject>();
        if (life != null && !isStuck)
        {
            life.GetComponentInParent<BossHealth>().TakeHit(other.gameObject);
            other.gameObject.SetActive(false); // destroy the life object
            Destroy(gameObject);        // destroy the stick
            return;
        }
    }

    void StickInPlace()
    {
        if (isStuck) return;
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }
}