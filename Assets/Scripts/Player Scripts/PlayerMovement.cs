using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputState))]

public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController characterController;

    [Header("Input")]
    private PlayerInputState inputStateScript;

    [Header("Movement Settings")]
    public float movementSpeed = 12f;
    public float jumpForce = 1.2f;
    public float gravity = -20f;

    private float verticalVelocity;
    private bool jumpRequested;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (inputStateScript == null)
        {
            inputStateScript = GetComponent<PlayerInputState>();
        }
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
            inputStateScript.JumpPerformed += HandleJumpPerformed;
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
            Debug.LogWarning("PlayerMovement: Missing required components. Please ensure CharacterController, Camera Transform, and PlayerInputState are assigned. CharacterController: " + characterController + ", Camera Transform: " + cameraTransform + ", PlayerInputState: " + inputStateScript);
            return;
        }

        if (inputStateScript.CurrentState != PlayerInputState.InputStates.Gameplay)
        {
            return;
        }

        Vector2 moveInput = inputStateScript.MovementInputData;

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        cameraForward.Normalize();

        Vector3 horizontalVelocity = (cameraRight * moveInput.x + cameraForward * moveInput.y) * movementSpeed;

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