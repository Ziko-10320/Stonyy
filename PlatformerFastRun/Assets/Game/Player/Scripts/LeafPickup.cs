using System.Collections;
using UnityEngine;

public class LeafPickup : MonoBehaviour
{
    [SerializeField] float respawnTime = 4f;

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        if (!player.isCheckingLeaf) return;
        player.PickupLeaf(gameObject);
        player.StartRespawn(gameObject, respawnTime);
    }
}