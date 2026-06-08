using UnityEngine;

public class WallPad : MonoBehaviour
{
    PlayerMovement player;
    bool playerWasAirborne;

    void OnTriggerStay2D(Collider2D other)
    {
        if (player == null)
            player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        bool airborne = !player.IsGrounded;

        if (playerWasAirborne && player.IsGrounded)
            player.FlipDirection();

        playerWasAirborne = airborne;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;
        player = null;
        playerWasAirborne = false;
    }
}