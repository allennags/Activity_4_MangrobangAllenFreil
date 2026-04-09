using UnityEngine;
using StarterAssets;

/// <summary>
/// Allows the player to interact with the pickup car and drive it.
/// Works with PlayerInteractor / IInteractable and the StarterAssets ThirdPersonController.
/// </summary>
[RequireComponent(typeof(CarControl))]
public class CarInteractable : MonoBehaviour, IInteractable
{
    [Header("Seat & Exit")]
    [Tooltip("Where the player will be placed when entering the car.")]
    public Transform driverSeat;

    [Tooltip("Where the player will be placed when exiting the car.")]
    public Transform exitPoint;

    [Header("Cameras")]
    [Tooltip("Camera used while driving the car (usually a child of the car).")]
    public Camera carCamera;

    [Header("Visuals")]
    [Tooltip("Optional: specific objects to hide while driving (e.g. the player mesh root). If empty, all renderers under the player root will be hidden.")]
    public GameObject[] objectsToHideWhileDriving;

    private CarControl carControl;
    private bool isOccupied = false;

    private PlayerInteractor currentInteractor;
    private GameObject currentPlayerRoot;
    private ThirdPersonController playerController;
    private StarterAssetsInputs playerInputs;
    private CharacterController characterController;
    private Camera playerCamera;
    private Renderer[] hiddenPlayerRenderers;

    // Cache to know if we are currently driving and should listen for exit input
    private bool wasDrivingLastFrame = false;

    private void Awake()
    {
        carControl = GetComponent<CarControl>();
        carControl.isPlayerDriving = false;
    }

    public string GetInteractionPrompt()
    {
        if (!isOccupied)
        {
            return "Press Interact to enter car";
        }
        else
        {
            return "Press Interact to exit car";
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!isOccupied)
        {
            EnterCar(interactor);
        }
        else if (interactor == currentInteractor)
        {
            ExitCar();
        }
    }

    private void EnterCar(PlayerInteractor interactor)
    {
        if (isOccupied) return;

        currentInteractor = interactor;
        // Let the interactor know which car it is driving so its UI button
        // can still trigger exit even when the camera raycast can't see the car.
        if (currentInteractor != null)
        {
            currentInteractor.SetCurrentCar(this);
        }
        // Find the main player object that has the movement controller.
        // First try to get it from the interactor's configured reference,
        // then fall back to a parent search, then to a global search.
        playerController = null;

        // Preferred: use the explicit reference from PlayerInteractor if available
        if (interactor != null && interactor.playerController != null)
        {
            playerController = interactor.playerController;
        }

        // Fallback: search up the hierarchy from the interactor
        if (playerController == null)
        {
            playerController = interactor.GetComponentInParent<ThirdPersonController>();
        }

        // Last resort: find any ThirdPersonController in the scene
        if (playerController == null)
        {
            playerController = FindObjectOfType<ThirdPersonController>();
        }

        if (playerController != null)
        {
            currentPlayerRoot = playerController.gameObject;
        }
        else
        {
            // Fallback: use the interactor object itself
            currentPlayerRoot = interactor.gameObject;
        }

        playerInputs = currentPlayerRoot.GetComponent<StarterAssetsInputs>();
        characterController = currentPlayerRoot.GetComponent<CharacterController>();

        // Cache current main camera (player camera).
        // Only switch cameras if a dedicated carCamera is assigned to avoid black screen on mobile.
        playerCamera = Camera.main;
        if (carCamera != null)
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            carCamera.gameObject.SetActive(true);
            carCamera.enabled = true;
            carCamera.targetDisplay = 0; // Ensure it renders to Display 1 (Game view)
        }

        // Disable player movement while driving
        if (playerController != null) playerController.enabled = false;
        if (characterController != null) characterController.enabled = false;
        if (playerInputs != null) playerInputs.move = Vector2.zero;

        // Feed the driver's inputs into the car so mobile joystick controls the car
        if (carControl != null)
        {
            carControl.driverInputs = playerInputs;
        }

        // Place player in the driver seat and parent to the seat so they
        // physically ride with the car wherever it moves.
        if (driverSeat != null && currentPlayerRoot != null)
        {
            currentPlayerRoot.transform.SetPositionAndRotation(driverSeat.position, driverSeat.rotation);
            currentPlayerRoot.transform.SetParent(driverSeat);
            currentPlayerRoot.transform.localPosition = Vector3.zero;
            currentPlayerRoot.transform.localRotation = Quaternion.identity;
        }
        else if (currentPlayerRoot != null)
        {
            // Fallback: parent directly to the car root if no driver seat is set
            currentPlayerRoot.transform.SetParent(transform);
        }

        // Hide the player mesh while driving so the character is not visible
        if (currentPlayerRoot != null)
        {
            // If specific objects are assigned, use them; otherwise hide everything under the player root
            System.Collections.Generic.List<Renderer> renderers = new System.Collections.Generic.List<Renderer>();

            if (objectsToHideWhileDriving != null && objectsToHideWhileDriving.Length > 0)
            {
                foreach (var obj in objectsToHideWhileDriving)
                {
                    if (obj == null) continue;
                    renderers.AddRange(obj.GetComponentsInChildren<Renderer>(true));
                }
            }
            else
            {
                renderers.AddRange(currentPlayerRoot.GetComponentsInChildren<Renderer>(true));
            }

            hiddenPlayerRenderers = renderers.ToArray();

            foreach (var renderer in hiddenPlayerRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        carControl.isPlayerDriving = true;
        isOccupied = true;
        wasDrivingLastFrame = true;
    }

    private void Update()
    {
        // While the car is occupied, listen for an exit request directly,
        // so we don't depend on the raycast-based PlayerInteractor.
        if (!isOccupied)
        {
            wasDrivingLastFrame = false;
            return;
        }

        bool wantsExit = false;

        // Prefer StarterAssetsInputs (works with the new Input System and mobile UI)
        if (carControl != null && carControl.driverInputs != null)
        {
            if (carControl.driverInputs.interact)
            {
                wantsExit = true;
                // Reset the flag so it behaves like a button press
                carControl.driverInputs.interact = false;
            }
        }
        else
        {
            // Fallback: direct keyboard E key
            if (Input.GetKeyDown(KeyCode.E))
            {
                wantsExit = true;
            }
        }

        if (wantsExit)
        {
            ExitCar();
        }
    }

    private void ExitCar()
    {
        if (!isOccupied || currentPlayerRoot == null) return;

        // Stop driving
        carControl.isPlayerDriving = false;
        if (carControl != null)
        {
            carControl.driverInputs = null;
        }

        // Restore cameras if we were using a dedicated car camera
        if (carCamera != null)
        {
            carCamera.enabled = false;
            carCamera.gameObject.SetActive(false);
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                playerCamera.gameObject.SetActive(true);
            }
        }

        // Unparent player from car and move to exit point
        currentPlayerRoot.transform.SetParent(null);
        if (exitPoint != null)
        {
            // Exit exactly at the configured point on the car
            currentPlayerRoot.transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
        }
        else
        {
            // Fallback: place the player beside the car root so they always
            // end up near the vehicle even if driverSeat/exitPoint aren't set.
            Vector3 sideOffset = -transform.right * 1.5f; // left side by default
            Vector3 exitPosition = transform.position + sideOffset;
            Quaternion exitRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            currentPlayerRoot.transform.SetPositionAndRotation(exitPosition, exitRotation);
        }

        // Show the player mesh again now that they are out of the car
        if (hiddenPlayerRenderers != null)
        {
            foreach (var renderer in hiddenPlayerRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
            hiddenPlayerRenderers = null;
        }

        // Re-enable movement
        if (playerController != null) playerController.enabled = true;
        if (characterController != null) characterController.enabled = true;

        // Clear references
        isOccupied = false;
        if (currentInteractor != null)
        {
            currentInteractor.SetCurrentCar(null);
        }
        currentInteractor = null;
        currentPlayerRoot = null;
        playerController = null;
        playerInputs = null;
        characterController = null;
    }
}
