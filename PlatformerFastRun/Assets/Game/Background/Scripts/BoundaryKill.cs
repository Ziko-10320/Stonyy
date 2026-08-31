using UnityEngine;

public class BoundaryKill : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger hit by: {other.name}");
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.InstantKill();
    }
}