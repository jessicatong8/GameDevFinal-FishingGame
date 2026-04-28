using UnityEngine;
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputState))]

public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement instance;
    public static PlayerMovement Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PlayerMovement>();
            }
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts.

    [Header("Camera Reference")]
    public Transform cameraTransform;
    [Header("Movement Settings")]
    public float movementSpeed = 12f;
    public float jumpForce = 1.2f;
    public float gravity = -20f;
    [SerializeField] private float rotationSharpness = 12f;

    private CharacterController characterController;
    private Animator animator;
    private Vector3 lastMoveDirection = Vector3.forward;
    private float verticalVelocity;
    private bool jumpRequested;

    private void Awake()
    {
        Instance = this;
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
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
        PlayerInputState.JumpPerformed += HandleJumpPerformed;
    }

    private void OnDisable()
    {
        PlayerInputState.JumpPerformed -= HandleJumpPerformed;
    }

    private void HandleJumpPerformed()
    {
        // Debug.Log("Jump input received. Current state: " + inputStateScript.CurrentState);
        jumpRequested = true;
    }

    private void Update()
    {
        if (characterController == null || cameraTransform == null || PlayerInputState.Instance == null)
        {
            DebugLogger.Instance.LogWarning("PlayerMovement: Missing required components. Please ensure CharacterController, Camera Transform, and PlayerInputState are in the scene. CharacterController: " + characterController + ", Camera Transform: " + cameraTransform + ", PlayerInputState.Instance: " + PlayerInputState.Instance);
            return;
        }

        if (PlayerInputState.Instance.CurrentState != PlayerInputState.InputStates.Gameplay) { return; }

        // constantly read movement input from player input state
        Vector2 moveInput = PlayerInputState.Instance.MovementInputData;

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
            // animator parameters for movement can be set here based on moveDirection and/or its magnitude
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
        animator?.SetFloat("moveSpeed", moveDirection.magnitude);
        characterController.Move(finalVelocity * Time.deltaTime);
    }
}