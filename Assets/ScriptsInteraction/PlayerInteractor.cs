using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 6f;
    public LayerMask interactableLayer;
    public Transform holdPoint;

    [Header("Debug")]
    public bool showDebug = true;

    private IInteractable currentInteractable;
    private GameObject heldObject;
    private Transform camTransform;
    
    [Header("Animation")]
    [Tooltip("Animator for the player character. Used to trigger arm pose when holding an item.")]
    public Animator animator;

    private int isHoldingItemHash;

    [Header("Procedural Arm Pose")]
    [Tooltip("Optional: lower-right arm bone to twist when holding an item.")]
    public Transform rightLowerArm;

    [Tooltip("Local Euler offset applied to the lower-right arm while holding an item.")]
    public Vector3 holdingArmLocalEuler = new Vector3(0f, 15f, -20f);

    private Quaternion rightLowerArmDefaultRotation;

    [Header("Player References")]
    [Tooltip("Main ThirdPersonController for the player. If left empty, it will be auto-found in the scene.")]
    public ThirdPersonController playerController;

    // When the player is driving a car, this will be set by CarInteractable
    private CarInteractable currentCar;

    private void Awake()
    {
        camTransform = Camera.main.transform;
        if (camTransform == null)
            Debug.LogError("Main Camera not found!");

        // Auto-find the main ThirdPersonController if not linked in the Inspector
        if (playerController == null)
        {
            playerController = FindObjectOfType<ThirdPersonController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerInteractor could not find a ThirdPersonController in the scene.");
            }
        }

        if (animator != null)
        {
            isHoldingItemHash = Animator.StringToHash("IsHoldingItem");
        }

        if (rightLowerArm != null)
        {
            rightLowerArmDefaultRotation = rightLowerArm.localRotation;
        }
    }

    private void Update()
    {
        // Always update what we're looking at
        CheckForInteractable();

        // Keyboard / gamepad interaction
        if (IsInteractPressed())
        {
            Debug.Log("🔵 E key pressed!");
            TryInteract();
        }
    }

    /// <summary>
    /// Tries to interact with the current target. Call this from
    /// keyboard, gamepad, UI button, or mobile touch.
    /// </summary>
    public void TryInteract()
    {
        if (heldObject != null && currentInteractable == null)
        {
            DropHeldObject();
            return;
        }

        if (currentInteractable != null)
        {
            Debug.Log($"🔵 Trying to interact with: {currentInteractable.GetInteractionPrompt()}");
            currentInteractable.Interact(this);
            Debug.Log("✅ Interact() was called on the object");
        }
        else
        {
            // If we are currently driving a car, treat this as an exit request
            if (currentCar != null)
            {
                Debug.Log("🚗 Interact triggered while driving car, forwarding to CarInteractable.");
                currentCar.Interact(this);
            }
            else
            {
                Debug.Log("❌ No interactable in range when Interact was triggered");
            }
        }
    }

    private void CheckForInteractable()
    {
        if (camTransform == null) return;

        Vector3 rayOrigin = camTransform.position + camTransform.forward * 0.5f;
        Ray ray = new Ray(rayOrigin, camTransform.forward);

        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>() 
                                      ?? hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    Debug.Log($"✅ Interactable found: {interactable.GetInteractionPrompt()} | Hit: {hit.collider.name}");
                }
                return;
            }
        }

        currentInteractable = null;
    }

    private void DropHeldObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        heldObject.transform.SetParent(null);
        heldObject = null;

        Debug.Log("✅ Object dropped");
    }

    // Public methods used by PickupObject
    public GameObject HeldObject => heldObject;
    public void SetHeldObject(GameObject obj)
    {
        heldObject = obj;

        if (animator != null && isHoldingItemHash != 0)
        {
            animator.SetBool(isHoldingItemHash, heldObject != null);
        }

        if (rightLowerArm != null)
        {
            if (heldObject != null)
            {
                rightLowerArm.localRotation = rightLowerArmDefaultRotation * Quaternion.Euler(holdingArmLocalEuler);
            }
            else
            {
                rightLowerArm.localRotation = rightLowerArmDefaultRotation;
            }
        }
    }

    // Called by CarInteractable when entering/exiting a car so UI/keyboard
    // interaction can be forwarded even if the raycast can't see the car.
    public void SetCurrentCar(CarInteractable car)
    {
        currentCar = car;
    }

    private bool IsInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif
        return Input.GetKeyDown(KeyCode.E);
    }
}