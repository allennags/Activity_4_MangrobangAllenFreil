using UnityEngine;

// Attach this to a UI Button and wire its OnClick to CallInteract().
public class MobileInteractButton : MonoBehaviour
{
    [Tooltip("Reference to the PlayerInteractor in the scene.")]
    public PlayerInteractor playerInteractor;

    // Called by the UI Button OnClick event
    public void CallInteract()
    {
        if (playerInteractor != null)
        {
            Debug.Log("📱 MobileInteractButton: CallInteract triggered");
            playerInteractor.TryInteract();
        }
        else
        {
            Debug.LogWarning("MobileInteractButton has no PlayerInteractor assigned.");
        }
    }
}
