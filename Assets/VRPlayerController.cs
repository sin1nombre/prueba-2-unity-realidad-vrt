using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera Look Settings")]
    public float lookSpeed = 90.0f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private float groundedTimer = 0f; // Ensures consistent jumping on Terrain

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleRotation();
        HandleMovementAndButtons();
    }

    void HandleRotation()
    {
        Vector2 lookInput = Vector2.zero;

        // 1. Read Arrow Keys on Keyboard (Camera Rotation)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed) lookInput.x += 1f;
            if (Keyboard.current.leftArrowKey.isPressed) lookInput.x -= 1f;
            if (Keyboard.current.upArrowKey.isPressed) lookInput.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) lookInput.y -= 1f;
        }

        // 2. Read Right Stick on Gamepad (GameSir T4 / Xbox Controller)
        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            if (rightStick.sqrMagnitude > 0.04f)
            {
                lookInput += rightStick;
            }
        }

        // Rotate Player body left/right (Yaw)
        transform.Rotate(Vector3.up * lookInput.x * lookSpeed * Time.deltaTime);

        // Rotate Camera up/down on PC Editor (Pitch)
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (cameraTransform != null && Mathf.Abs(lookInput.y) > 0.01f)
        {
            cameraPitch -= lookInput.y * lookSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
            cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }
        #endif
    }

    void HandleMovementAndButtons()
    {
        // Track grounded state reliably with a small grace timer
        if (controller.isGrounded)
        {
            groundedTimer = 0.15f;
            if (velocity.y < 0f)
            {
                velocity.y = -2f; // Keep character firmly snapped to the terrain
            }
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }

        Vector2 moveInput = Vector2.zero;
        bool jumpPressed = false;
        bool isRunning = false;

        // 1. Read ONLY WASD on Keyboard for movement
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
            if (Keyboard.current.leftShiftKey.isPressed) isRunning = true;
        }

        // 2. Read Left Stick and Buttons on Gamepad (GameSir T4 / Xbox Controller)
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            if (leftStick.sqrMagnitude > 0.04f)
            {
                moveInput += leftStick;
            }

            // Button A (South) or Right Shoulder (RB) -> Jump
            if (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                jumpPressed = true;
            }

            // Left Shoulder (LB), Left Trigger (LT) or Button X (West) -> Sprint
            if (Gamepad.current.leftShoulder.isPressed || Gamepad.current.leftTrigger.isPressed || Gamepad.current.buttonWest.isPressed)
            {
                isRunning = true;
            }
        }

        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        // Calculate horizontal direction based on where the camera is facing
        Transform referenceTransform = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = referenceTransform.forward;
        Vector3 right = referenceTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        float currentSpeed = isRunning ? moveSpeed * 1.8f : moveSpeed;
        Vector3 horizontalVelocity = (forward * moveInput.y + right * moveInput.x) * currentSpeed;

        // Trigger Jump if on the ground
        if (jumpPressed && groundedTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            groundedTimer = 0f;
        }

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine Horizontal and Vertical movement in a SINGLE Move() call
        Vector3 finalMovement = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        controller.Move(finalMovement * Time.deltaTime);
    }
}