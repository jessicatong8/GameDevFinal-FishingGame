using UnityEngine;
using UnityEngine.InputSystem;

// Handles equip/cast/reel state and synchronizes fishing visuals and rope behavior.
public class PlayerFishing : MonoBehaviour
{
    [Header("Input System Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string fishingActionMapName = "Fishing";
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private string castActionName = "-Cast";
    [SerializeField] private string reelActionName = "Reel";
    [SerializeField] private string menuActionName = "Menu";
    [SerializeField] private string uiCancelActionName = "Cancel";

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject fishingRod;
    [SerializeField] private GameObject fishingLineObject;
    [SerializeField] private GameObject fishingHookObject;
    [SerializeField] private VerletLine fishingLine;
    [SerializeField] private Player playerScript;

    private InputAction interactAction;
    private InputAction castAction;
    private InputAction reelAction;
    private InputAction menuAction;
    private InputAction uiCancelAction;
    private PlayerInput playerInput;
    private bool usesPlayerInputMaps;

    public bool IsFishing { get; private set; }
    private bool alreadyCast;
    private bool isReeling;
    private bool hasEnteredReelState;
    private bool isPaused;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (fishingLine == null)
        {
            fishingLine = GetComponentInChildren<VerletLine>(true);
        }

        if (playerScript == null)
        {
            playerScript = GetComponent<Player>();
        }

        InitializeInputActions();
        SetFishingState(false);
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
        HandlePauseInput();

        if (isPaused)
        {
            return;
        }

        HandleFishingInput();
        HandleReelStateCompletion();
    }

    private void HandlePauseInput()
    {
        if (!isPaused)
        {
            if (MenuPressed())
            {
                SetPaused(true);
            }

            return;
        }

        if (UiCancelPressed())
        {
            SetPaused(false);
        }
    }

    private void HandleFishingInput()
    {
        if (InteractPressed())
        {
            if (!IsFishing)
            {
                SetFishingState(true);
            }
            else if (!alreadyCast && !isReeling)
            {
                SetFishingState(false);
            }
        }

        if (CastPressed() && IsFishing && !alreadyCast && !isReeling)
        {
            if (animator != null)
            {
                animator.SetTrigger("cast");
            }

            fishingLine?.TriggerCast();
            alreadyCast = true;
        }

        if (ReelPressed() && IsFishing && alreadyCast && !isReeling)
        {
            if (animator != null)
            {
                animator.SetTrigger("reel");
            }

            fishingLine?.TriggerReel();
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
        IsFishing = active;

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

        fishingLine?.SetEquippedFromController(active);

        if (!isPaused)
        {
            SwitchActionMap(active ? fishingActionMapName : playerActionMapName);
        }

        if (playerScript != null)
        {
            playerScript.enabled = !active;
        }
    }

    private void SetPaused(bool active)
    {
        isPaused = active;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(active);
        }

        if (active)
        {
            SwitchActionMap(uiActionMapName);
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = 1f;
        SwitchActionMap(IsFishing ? fishingActionMapName : playerActionMapName);
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

    private bool MenuPressed()
    {
        if (menuAction != null)
        {
            return menuAction.WasPressedThisFrame();
        }

        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private bool UiCancelPressed()
    {
        if (uiCancelAction != null)
        {
            return uiCancelAction.WasPressedThisFrame();
        }

        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private void SwitchActionMap(string actionMapName)
    {
        if (playerInput == null)
        {
            return;
        }

        if (playerInput.currentActionMap != null && playerInput.currentActionMap.name == actionMapName)
        {
            return;
        }

        playerInput.SwitchCurrentActionMap(actionMapName);
    }

    private void InitializeInputActions()
    {
        playerInput = GetComponent<PlayerInput>();
        usesPlayerInputMaps = playerInput != null;

        if (inputActionsAsset == null)
        {
            if (playerInput != null)
            {
                inputActionsAsset = playerInput.actions;
            }
        }

        if (inputActionsAsset == null)
        {
            return;
        }

        InputActionMap playerMap = inputActionsAsset.FindActionMap(playerActionMapName, false);
        if (playerMap == null)
        {
            return;
        }

        InputActionMap uiMap = inputActionsAsset.FindActionMap(uiActionMapName, false);

        interactAction = playerMap.FindAction(interactActionName, false);
        castAction = playerMap.FindAction(castActionName, false);
        menuAction = playerMap.FindAction(menuActionName, false);
        uiCancelAction = uiMap != null ? uiMap.FindAction(uiCancelActionName, false) : null;

        InputActionMap fishingMap = inputActionsAsset.FindActionMap(fishingActionMapName, false);
        reelAction = fishingMap != null ? fishingMap.FindAction(reelActionName, false) : null;

        // Backward-compatible aliases for older action assets.
        if (castAction == null)
        {
            castAction = playerMap.FindAction("Cast", false);
        }
        if (reelAction == null)
        {
            reelAction = playerMap.FindAction("Next", false);
        }

        EnableInputActions();
    }

    private void EnableInputActions()
    {
        if (usesPlayerInputMaps)
        {
            return;
        }

        interactAction?.Enable();
        castAction?.Enable();
        reelAction?.Enable();
        menuAction?.Enable();
        uiCancelAction?.Enable();
    }

    private void DisableInputActions()
    {
        if (usesPlayerInputMaps)
        {
            return;
        }

        interactAction?.Disable();
        castAction?.Disable();
        reelAction?.Disable();
        menuAction?.Disable();
        uiCancelAction?.Disable();
    }
}
