using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputState))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform;
    [Header("Movement Settings")]
    public float movementSpeed = 12f;
    public float jumpForce = 1.2f;
    public float gravity = -20f;
    [SerializeField] private float rotationSharpness = 12f;
    private CharacterController characterController;
    private PlayerInputState inputStateScript;
    private float verticalVelocity;
    private bool jumpRequested;
    private Vector3 lastMoveDirection = Vector3.forward;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputStateScript = GetComponent<PlayerInputState>();
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        if (inputStateScript != null)
        {
            // inputStateScript.JumpPerformed += HandleJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (inputStateScript != null)
        {
            inputStateScript.JumpPerformed -= HandleJumpPerformed;
        }
    }

    private void HandleJumpPerformed()
    {
        // Debug.Log("Jump input received. Current state: " + inputStateScript.CurrentState);
        jumpRequested = true;
    }

    private void Update()
    {
        if (characterController == null || cameraTransform == null || inputStateScript == null)
        {
            DebugLogger.Instance.LogWarning("PlayerMovement: Missing required components. Please ensure CharacterController, Camera Transform, and PlayerInputState are assigned. CharacterController: " + characterController + ", Camera Transform: " + cameraTransform + ", PlayerInputState: " + inputStateScript);
            return;
        }

        if (inputStateScript.CurrentState != PlayerInputState.InputStates.Gameplay) { return; }

        // constantly read movement input from player input state
        Vector2 moveInput = inputStateScript.MovementInputData;

        // constantly read camera movement input from player input state
        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        cameraForward.Normalize();

        Vector3 moveDirection = cameraRight * moveInput.x + cameraForward * moveInput.y;
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = moveDirection.normalized;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSharpness * Time.deltaTime);

        Vector3 horizontalVelocity = moveDirection * movementSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (characterController.isGrounded && jumpRequested)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpRequested = false;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(finalVelocity * Time.deltaTime);
    }
}