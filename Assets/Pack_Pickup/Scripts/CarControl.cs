using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class CarControl : MonoBehaviour
{
    public float enginePower = 2000.0f; // Engine power
    public float turnSpeed = 25.0f; // Maximum turn speed
    public float turnSmoothness = 5.0f; // Turn smoothness
    public Transform[] wheels; // Array of Wheel Collider components
    public Transform[] wheelMeshes; // Array of wheel meshes
    public Transform centerOfMass; // Center of Mass point
    public GameObject steeringWheel;

    [Header("Control State")]
    [Tooltip("When true, the car reads player input and can be driven.")]
    public bool isPlayerDriving = false;

    [Tooltip("Optional: StarterAssetsInputs from the driving player (used for mobile/joystick input). If null, old Input.GetAxis is used.")]
    public StarterAssetsInputs driverInputs;

    private Rigidbody rb; // Car's Rigidbody component
    private float currentTurnAngle = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition; // Set center of mass
    }

    void FixedUpdate()
    {
        if (!isPlayerDriving)
        {
            return;
        }

        float horizontalInput;
        float verticalInput;

        if (driverInputs != null)
        {
            // Use player move input (x = left/right, y = forward/back)
            horizontalInput = driverInputs.move.x;
            verticalInput = driverInputs.move.y;
        }
        else
        {
            // Fallback to old keyboard input (editor / PC)
            horizontalInput = Input.GetAxis("Horizontal"); // Turn input (right/left arrow keys or A/D keys)
            verticalInput = Input.GetAxis("Vertical"); // Acceleration input (up/down arrow keys or W/S keys)
        }

        // Calculate turn angle
        float targetTurnAngle = horizontalInput * turnSpeed;
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurnAngle, Time.deltaTime * turnSmoothness);

        steeringWheel.transform.localEulerAngles = new Vector3(-64, 0, currentTurnAngle * 3);



        // Wheels rotation and steering
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelCollider wheelCollider = wheels[i].GetComponent<WheelCollider>();
            if (i < 2) // First two wheels steer
            {
                wheelCollider.steerAngle = currentTurnAngle;
            }
            else
            {
                wheelCollider.steerAngle = 0f; // Other two wheels remain straight
            }

            // Wheels driving (forward/backward movement)
            wheelCollider.motorTorque = verticalInput * enginePower;

            // Rotate wheel meshes
            Quaternion wheelRotation;
            Vector3 wheelPosition;
            wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMeshes[i].position = wheelPosition;
            wheelMeshes[i].rotation = wheelRotation;
        }
    }
}
