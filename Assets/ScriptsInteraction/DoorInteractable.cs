using UnityEngine;
using DoorScript;

// Makes the Free Wood Door Pack "Door" script work with PlayerInteractor / IInteractable.
[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("Reference to the Door script that handles rotation and sounds.")]
    public Door door;

    [Tooltip("Prompt text shown when looking at the door.")]
    public string interactionPrompt = "Open / Close Door";

    private void Reset()
    {
        // Auto-assign door reference if possible
        if (door == null)
        {
            door = GetComponent<Door>() ?? GetComponentInParent<Door>();
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (door == null)
        {
            door = GetComponent<Door>() ?? GetComponentInParent<Door>();
        }

        if (door != null)
        {
            door.OpenDoor();
        }
        else
        {
            Debug.LogWarning($"DoorInteractable on {name} has no Door reference set.");
        }
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
}