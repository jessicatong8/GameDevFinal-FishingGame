using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController controller;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";

    [Header("Movement")]
    public float acceleration = 12f;
    public float dragFactor = 0.95f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    private Vector3 velocity = Vector3.zero;
    private float verticalVelocity;

    private PlayerInput playerInput;
    private bool usesPlayerInputMaps;
    private InputAction moveAction;
    private InputAction jumpAction;

    private void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        InitializeInputActions();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        if (usesPlayerInputMaps)
        {
            return;
        }

        moveAction?.Disable();
        jumpAction?.Disable();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (usesPlayerInputMaps
            && playerInput != null
            && (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerActionMapName))
        {
            return;
        }

        if (controller == null || cameraTransform == null)
        {
            return;
        }

        Vector2 moveInput = GetMoveInput();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        cameraForward.Normalize();

        Vector3 xAccel = cameraRight * moveInput.x * Time.deltaTime * acceleration;
        Vector3 zAccel = cameraForward * moveInput.y * Time.deltaTime * acceleration;

        velocity += xAccel + zAccel;
        velocity *= dragFactor;

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (JumpPressed())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalVelocity = velocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private Vector2 GetMoveInput()
    {
        if (moveAction != null)
        {
            return Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
        }

        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    private bool JumpPressed()
    {
        if (jumpAction != null)
        {
            return jumpAction.WasPressedThisFrame();
        }

        return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    private void InitializeInputActions()
    {
        playerInput = GetComponent<PlayerInput>();
        usesPlayerInputMaps = playerInput != null;

        if (inputActionsAsset == null && playerInput != null)
        {
            inputActionsAsset = playerInput.actions;
        }

        if (inputActionsAsset == null)
        {
            return;
        }

        InputActionMap actionMap = inputActionsAsset.FindActionMap(playerActionMapName, false);
        if (actionMap == null)
        {
            return;
        }

        moveAction = actionMap.FindAction(moveActionName, false);
        jumpAction = actionMap.FindAction(jumpActionName, false);

        if (!usesPlayerInputMaps)
        {
            moveAction?.Enable();
            jumpAction?.Enable();
        }
    }
}