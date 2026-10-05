using UnityEngine;

// Put this on any GameObject with a Collider2D (set to "Is Trigger") that you want
// to act as a tutorial checkpoint. Drag your tutorial panel's CanvasGroup into the slot below.
[RequireComponent(typeof(Collider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("The CanvasGroup on your tutorial panel (empty GameObject holding your text/images)")]
    public CanvasGroup panelToShow;

    [Tooltip("Which tag counts as the player? Leave as 'Player' unless yours is different.")]
    public string playerTag = "Player";

    [Tooltip("If true, this trigger only fires once then disables itself")]
    public bool triggerOnce = true;

    private bool hasFired = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasFired && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        TutorialManager.Instance.ShowPanel(panelToShow);
        hasFired = true;
    }
}