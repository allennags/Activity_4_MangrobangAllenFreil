using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private bool isHeld = false;
    private PlayerInteractor currentInteractor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!isHeld && interactor.HeldObject == null)
        {
            // PICK UP
            isHeld = true;
            currentInteractor = interactor;

            rb.isKinematic = true;
            transform.SetParent(interactor.holdPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            interactor.SetHeldObject(gameObject);

            Debug.Log("🎉 CUBE SUCCESSFULLY PICKED UP and moved to hold point!");
        }
        else if (isHeld)
        {
            // DROP
            Drop(interactor);
        }
    }

    private void Drop(PlayerInteractor interactor)
    {
        isHeld = false;
        currentInteractor = null;

        rb.isKinematic = false;
        transform.SetParent(null);

        interactor.SetHeldObject(null);

        Debug.Log("✅ Cube dropped");
    }

    public string GetInteractionPrompt()
    {
        return isHeld ? "Drop" : "Pick Up";
    }
}