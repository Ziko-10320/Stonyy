using System.Collections;
using UnityEngine;

public class StickPickup : MonoBehaviour
{
    [SerializeField] float respawnTime = 3.5f;

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        if (!player.isCheckingStick) return;
        player.PickupStick(gameObject);
        player.StartRespawn(gameObject, respawnTime);
    }
}