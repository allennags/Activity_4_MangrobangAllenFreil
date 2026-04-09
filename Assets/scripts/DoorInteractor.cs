using UnityEngine;
using DoorScript;

// Attach this to your player camera (or character) to interact with doors.
public class DoorInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;      // How far you can interact with a door
    public KeyCode interactKey = KeyCode.E;  // Key to press to open/close
    public LayerMask interactLayerMask = ~0; // Optional: restrict what can be hit

    void Update()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
        {
            // Look for a Door component on the hit object or its parents
            Door door = hit.collider.GetComponentInParent<Door>();
            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }
}
