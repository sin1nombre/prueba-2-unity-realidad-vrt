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
    private Transform pitchPivot; 
    private Vector3 velocity;
    private float cameraPitch = 0f;
    private float groundedTimer = 0f;

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            GameObject pivotObj = new GameObject("CameraPitchPivot");
            pitchPivot = pivotObj.transform;
            pitchPivot.SetParent(transform, false);
            pitchPivot.localPosition = cameraTransform.localPosition;
            pitchPivot.localRotation = Quaternion.identity;

            cameraTransform.SetParent(pitchPivot, false);
            cameraTransform.localPosition = Vector3.zero;
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

        if (Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed) lookInput.x += 1f;
            if (Keyboard.current.leftArrowKey.isPressed) lookInput.x -= 1f;
            if (Keyboard.current.upArrowKey.isPressed) lookInput.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) lookInput.y -= 1f;
        }

        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            if (rightStick.sqrMagnitude > 0.04f)
            {
                lookInput += rightStick;
            }
        }

        transform.Rotate(Vector3.up * lookInput.x * lookSpeed * Time.deltaTime);

        if (pitchPivot != null && Mathf.Abs(lookInput.y) > 0.01f)
        {
            cameraPitch -= lookInput.y * lookSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
            pitchPivot.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }
    }

    void HandleMovementAndButtons()
    {
        if (controller.isGrounded)
        {
            groundedTimer = 0.15f;
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }

        Vector2 moveInput = Vector2.zero;
        bool jumpPressed = false;
        bool isRunning = false;

        // 1. Read WASD on Keyboard
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
            if (Keyboard.current.leftShiftKey.isPressed) isRunning = true;
        }

        // 2. Read Left Stick and Buttons on Gamepad
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            if (leftStick.sqrMagnitude > 0.04f)
            {
                moveInput += leftStick;
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                jumpPressed = true;
            }

            if (Gamepad.current.leftShoulder.isPressed || Gamepad.current.leftTrigger.isPressed || Gamepad.current.buttonWest.isPressed)
            {
                isRunning = true;
            }
        }

        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        Transform referenceTransform = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = referenceTransform.forward;
        Vector3 right = referenceTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        float currentSpeed = isRunning ? moveSpeed * 1.8f : moveSpeed;
        Vector3 horizontalVelocity = (forward * moveInput.y + right * moveInput.x) * currentSpeed;

        if (jumpPressed && groundedTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            groundedTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        controller.Move(finalMovement * Time.deltaTime);
    }
}