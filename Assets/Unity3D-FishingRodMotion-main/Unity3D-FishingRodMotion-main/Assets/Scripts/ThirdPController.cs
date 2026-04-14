using UnityEngine;
using UnityEngine.InputSystem;

// Standard third-person character controller with fishing state toggles.
public class ThirdPController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator animator;
    public GameObject fishingRod;
    public GameObject fishingLineObject;
    public GameObject fishingHookObject;
    public VerletLine fishingLine;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSmoothTime = 0.1f;
    public bool rotateCharacterToMovement = false;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Input System Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private string castActionName = "Attack";
    [SerializeField] private string reelActionName = "Next";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction castAction;
    private InputAction reelAction;

    private float verticalVelocity;
    private float turnSmoothVelocity;

    public bool alreadyCast = false;
    public bool isFishing = false;
    public bool isReeling = false;
    private bool hasEnteredReelState = false;

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

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (fishingLine == null)
        {
            fishingLine = GetComponentInChildren<VerletLine>(true);
        }

        InitializeInputActions();
        SetFishingState(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void Update()
    {
        HandleFishingInput();

        if (!isFishing)
        {
            HandleMovement();
        }
        else
        {
            // Keep gravity/grounding stable while movement is paused for fishing.
            if (controller != null)
            {
                if (controller.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }

                verticalVelocity += gravity * Time.deltaTime;
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            }
        }

        HandleReelStateCompletion();
    }

    private void HandleMovement()
    {
        if (controller == null || cameraTransform == null)
        {
            return;
        }

        Vector2 moveInput = GetMoveInput();
        bool jumpPressed = JumpPressed();
        bool sprintHeld = SprintHeld();

        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 worldMove = (cameraRight * inputDirection.x + cameraForward * inputDirection.z);
        if (worldMove.sqrMagnitude > 1f)
        {
            worldMove.Normalize();
        }

        if (worldMove.sqrMagnitude > 0.001f)
        {
            float speed = sprintHeld ? sprintSpeed : moveSpeed;
            Vector3 horizontalVelocity = worldMove * speed;

            if (rotateCharacterToMovement)
            {
                float targetAngle = Mathf.Atan2(worldMove.x, worldMove.z) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }

            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
        else
        {
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        if (animator != null)
        {
            // Drive animation from actual movement direction relative to character facing.
            Vector3 localMove = transform.InverseTransformDirection(worldMove);
            animator.SetFloat("velX", localMove.x);
            animator.SetFloat("velY", localMove.z);
            animator.SetBool("isGrounded", controller.isGrounded);
        }
    }

    private void HandleFishingInput()
    {
        if (InteractPressed())
        {
            if (!isFishing)
            {
                SetFishingState(true);
            }
            else if (!alreadyCast && !isReeling)
            {
                SetFishingState(false);
            }
        }

        if (CastPressed() && isFishing && !alreadyCast && !isReeling)
        {
            if (animator != null)
            {
                animator.SetTrigger("cast");
            }

            if (fishingLine != null)
            {
                fishingLine.TriggerCast();
            }

            alreadyCast = true;
        }

        if (ReelPressed() && isFishing && alreadyCast && !isReeling)
        {
            if (animator != null)
            {
                animator.SetTrigger("reel");
            }

            if (fishingLine != null)
            {
                fishingLine.TriggerReel();
            }

            isReeling = true;
            hasEnteredReelState = false;
        }
    }

    private void HandleReelStateCompletion()
    {
        if (!isReeling)
        {
            return;
        }

        if (animator == null)
        {
            alreadyCast = false;
            isReeling = false;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool inReelState = stateInfo.IsName("Reel In");

        if (inReelState)
        {
            hasEnteredReelState = true;
        }

        if (hasEnteredReelState && !inReelState)
        {
            alreadyCast = false;
            isReeling = false;
            hasEnteredReelState = false;
        }
    }

    private void SetFishingState(bool active)
    {
        isFishing = active;

        if (!active)
        {
            alreadyCast = false;
            isReeling = false;
            hasEnteredReelState = false;
        }

        if (animator != null)
        {
            animator.SetBool("startFishing", active);
        }

        if (fishingRod != null)
        {
            fishingRod.SetActive(active);
        }

        if (fishingLineObject != null)
        {
            fishingLineObject.SetActive(active);
        }

        if (fishingHookObject != null)
        {
            fishingHookObject.SetActive(active);
        }

        if (fishingLine != null)
        {
            fishingLine.SetEquippedFromController(active);
        }
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
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
        }

        Vector2 keyboardMove = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        Vector2 gamepadMove = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        return Vector2.ClampMagnitude(keyboardMove + gamepadMove, 1f);
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

    private bool SprintHeld()
    {
        if (sprintAction != null)
        {
            return sprintAction.IsPressed();
        }

        return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            || (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);
    }

    private bool InteractPressed()
    {
        if (interactAction != null)
        {
            return interactAction.WasPressedThisFrame();
        }

        return (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
    }

    private bool CastPressed()
    {
        if (castAction != null)
        {
            return castAction.WasPressedThisFrame();
        }

        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);
    }

    private bool ReelPressed()
    {
        if (reelAction != null)
        {
            return reelAction.WasPressedThisFrame();
        }

        return (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);
    }

    private void InitializeInputActions()
    {
        if (inputActionsAsset == null)
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                inputActionsAsset = playerInput.actions;
            }
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
        sprintAction = actionMap.FindAction(sprintActionName, false);
        interactAction = actionMap.FindAction(interactActionName, false);
        castAction = actionMap.FindAction(castActionName, false);
        reelAction = actionMap.FindAction(reelActionName, false);

        EnableInputActions();
    }

    private void EnableInputActions()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        sprintAction?.Enable();
        interactAction?.Enable();
        castAction?.Enable();
        reelAction?.Enable();
    }

    private void DisableInputActions()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        sprintAction?.Disable();
        interactAction?.Disable();
        castAction?.Disable();
        reelAction?.Disable();
    }
}
