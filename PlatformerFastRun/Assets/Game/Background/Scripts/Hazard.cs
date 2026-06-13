using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] bool instantKill = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        if (instantKill)
            health.TakeDamage(health.MaxHealth);
        else
            health.TakeDamage(damage);
    }

   
}