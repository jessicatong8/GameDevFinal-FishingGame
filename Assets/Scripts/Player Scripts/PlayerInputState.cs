using System;
using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(PlayerInput))]
// [RequireComponent(typeof(PlayerMovement))]
// [RequireComponent(typeof(PlayerFishing))]
public class PlayerInputState : MonoBehaviour
{
    public static PlayerInputState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PlayerInputState>();
            }
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts.

    public enum InputStates
    {
        Gameplay,
        Fishing,
        Menu
    }
    // Gameplay Camera and Movement Inputs 
    public Vector2 MovementInputData { get; private set; }
    public Vector2 LookInputData { get; private set; }
    public float ZoomInputData { get; private set; }

    // Gameplay Input Events
    public static event Action InteractPerformed;
    public static event Action JumpPerformed;
    // Fishing Input Events
    public static event Action HookPerformed;
    public static event Action MashPerformed;
    public static event Action CatchConfirmPerformed;
    public static event Action LevelConfirmPerformed;
    public static event Action ReelLeftPerformed;
    public static event Action ReelRightPerformed;
    public static event Action AbortPerformed;
    public static event Action DebugPerformed;
    public static event Action CycleHatPerformed;
    public static event Action MenuTogglePerformed;
    public InputStates CurrentState => currentState;
    [SerializeField] private InputStates currentState = InputStates.Gameplay;
    private static PlayerInputState instance;
    private FishingManager fishingManager;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetState(InputStates.Gameplay);
        fishingManager = FishingManager.Instance;
        if (fishingManager == null)
        {
            DebugLogger.Instance.LogError("PlayerInputState: No FishingManager instance found in scene.");
        }
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public InputStates GetCurrentInputState()
    {
        return currentState;
    }
    public void SetState(InputStates state)
    {
        // DebugLogger.Instance.Log($"PlayerInputState: Switching to state: {currentState}");
        // DebugLogger.Instance.LogMethodCall("PlayerInputState.SetState", $"{currentState} -> {state}");
        currentState = state;
        ClearInputs();

        switch (currentState)
        {
            case InputStates.Gameplay:
                PlayerMovement.Instance.enabled = true;
                LockCursor(true);
                break;
            case InputStates.Fishing:
                PlayerMovement.Instance.enabled = false;
                LockCursor(true);
                break;
            case InputStates.Menu:
                PlayerMovement.Instance.enabled = false;
                LockCursor(false);
                break;
            default:
                DebugLogger.Instance.LogWarning("PlayerInputState: Unhandled state: " + currentState);
                break;
        }
    }
    public void OnMove(InputValue value)
    {
        if (currentState != InputStates.Gameplay) return;

        MovementInputData = value.Get<Vector2>();
    }
    public void OnLook(InputValue value)
    {
        if (currentState != InputStates.Gameplay) return;
        LookInputData = value.Get<Vector2>();
    }
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentState != InputStates.Gameplay) return;
        // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnJump", "-> !JumpPerformed");
        JumpPerformed?.Invoke();
    }
    public void OnZoom(InputValue value)
    {
        if (currentState != InputStates.Gameplay) return;
        OnZoomScroll(value);
    }
    public void OnZoomScroll(InputValue value)
    {
        ZoomInputData = value.Get<Vector2>().y;
    }
    // TODO - TURN OFF FOR ACTUAL BUILD
    public void OnCycleHat(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentState == InputStates.Gameplay)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnCycleHat", "-> !CycleHatPerformed");
            CycleHatPerformed?.Invoke();
        }
    }
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentState == InputStates.Gameplay)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnInteract", "-> !InteractPerformed");
            InteractPerformed?.Invoke();
        }
    }
    public void OnConfirm(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnConfirm", "-> !ConfirmCatchPerformed");
            CatchConfirmPerformed?.Invoke();
        }
        else if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.LevelUpPresentation)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnConfirmLevelUp", "-> !ConfirmLevelUpPerformed");
            LevelConfirmPerformed?.Invoke();
        }
        else
        {
            DebugLogger.Instance.LogWarning("PlayerInputState.OnConfirm: Confirm input received but not in a presentation state.");
        }
    }
    public void OnHook(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.HookWindow)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnHook", "-> !HookPerformed\nHookWindow success");
            HookPerformed?.Invoke();
        }
    }
    public void OnMash(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnMash","-> !MashPerformed");
            MashPerformed?.Invoke();
        }
    }
    public void OnReelLeft(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
        {
            // Debug.Log("OnReelLeft called with value: " + value);
            ReelLeftPerformed?.Invoke();
        }
    }
    public void OnReelRight(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
        {
            // Debug.Log("OnReelRight called with value: " + value);
            ReelRightPerformed?.Invoke();
        }
    }
    public void OnAbort(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Fishing) return;

        // acts as confirm catch during catch presentation, otherwise abort for any fishing state (casting, hook window, reeling)
        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnAbort", "-> !ConfirmCatchPerformed");
            CatchConfirmPerformed?.Invoke();
        }
        else if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.LevelUpPresentation)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnAbort", "-> !ConfirmLevelUpPerformed");
            LevelConfirmPerformed?.Invoke();
        }
        else
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnAbort", "-> !AbortPerformed");
            AbortPerformed?.Invoke();
            FishingManager.Instance.ReturnToGameplay("PlayerAborted");
        }
    }
    public void OnMenuToggle(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentState != InputStates.Gameplay) return;

        if (currentState == InputStates.Menu)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnMenu", "-> !MenuTogglePerformed");
            MenuTogglePerformed?.Invoke();
        }
    }
    public void OnDebug(InputValue value)
    {
        if (!value.isPressed) return;

        // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnDebug", "-> !DebugPerformed");
        DebugPerformed?.Invoke();
    }
    private void LockCursor(bool shouldLock)
    {
        if (shouldLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    private void LateUpdate()
    {
        LookInputData = Vector2.zero;
        ZoomInputData = 0f;
    }
    private void ClearInputs()
    {
        MovementInputData = Vector2.zero;
        LookInputData = Vector2.zero;
        ZoomInputData = 0f;
    }
}