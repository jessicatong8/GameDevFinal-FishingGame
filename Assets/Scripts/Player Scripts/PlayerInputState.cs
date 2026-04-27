using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerFishing))]
public class PlayerInputState : MonoBehaviour
{
    private static PlayerInputState instance;
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
        Fishing
    }
    [SerializeField] private InputStates currentState = InputStates.Gameplay;

    // Gameplay Camera and Movement Inputs 
    public Vector2 MovementInputData { get; private set; }
    public Vector2 LookInputData { get; private set; }
    public float ZoomInputData { get; private set; }

    // Gameplay Input Events
    public static event Action InteractPerformed;
    public static event Action JumpPerformed;
    // Menu? 

    // Fishing Input Events
    public static event Action HookPerformed;
    public static event Action MashPerformed;
    public static event Action ConfirmCatchPerformed;
    public event Action ReelLeftPerformed;
    public event Action ReelRightPerformed;
    public static event Action AbortPerformed;
    public static event Action DebugPerformed;
    // Menu?


    public InputStates CurrentState => currentState;
    [SerializeField] private FishingManager fishingManager;
    private PlayerMovement movementScript;
    // private PlayerFishing fishingScript;


    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DebugLogger.Instance.Log($"PlayerInputState initialized with state: {currentState}");
        movementScript = GetComponent<PlayerMovement>();
        // fishingScript = GetComponent<PlayerFishing>();
        // menuScript = GetComponent<PlayerMenu>();
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
                movementScript.enabled = true;
                break;
            case InputStates.Fishing:
                movementScript.enabled = false;
                break;
            default:
                DebugLogger.Instance.LogWarning("PlayerInputState: Unhandled state: " + currentState);
                break;
        }
    }

    public void OnMove(InputValue value)
    {
        if (currentState != InputStates.Gameplay) { return; }

        MovementInputData = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (currentState != InputStates.Gameplay && currentState != InputStates.Fishing) { return; }

        LookInputData = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Gameplay)
        {
            // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnJump", "-> !JumpPerformed");
            JumpPerformed?.Invoke();
        }
    }

    public void OnZoom(InputValue value)
    {
        if (currentState == InputStates.Gameplay)
        {
            OnZoomScroll(value);
        }
    }

    public void OnZoomScroll(InputValue value)
    {
        // if (currentState != InputStates.Menu) { return; }
        ZoomInputData = value.Get<Vector2>().y;
    }

    public void OnZoomToggle(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Gameplay)
        {
            // Toggle zoom between 1st person and 3rd person
            DebugLogger.Instance.LogMethodCall("PlayerInputState.OnZoomToggle", $"-> Current zoom input: {(ZoomInputData == 0f ? "1 (3rd P)" : "0 (1st P)")}.");
            ZoomInputData = ZoomInputData == 0f ? 1f : 0f;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Gameplay)
        {
            // In gameplay: starts fishing if possible. In fishing: allows catch confirmation listeners to respond.
            DebugLogger.Instance.LogMethodCall("PlayerInputState.OnInteract", "-> !InteractPerformed");
            InteractPerformed?.Invoke();
        }
    }

    public void OnConfirmCatch(InputValue value)
    {
        if (!value.isPressed) { return; }

        FishingManager fishingManagerInstance = FishingManager.Instance;
        if (fishingManagerInstance == null)
        {
            return;
        }

        if (currentState == InputStates.Fishing || fishingManagerInstance.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
        {
            DebugLogger.Instance.LogMethodCall("PlayerInputState.OnConfirmCatch", "-> !ConfirmCatchPerformed");
            FishingManager.Instance.ReturnToIdle("Catch Confirmed");
        }
    }

    public void OnHook(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing && fishingManager != null)
        {
            if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.HookWindow)
            {
                // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnHook", "-> !HookPerformed\nHookWindow success");
                HookPerformed?.Invoke();
            }
        }

    }
    public void OnMash(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing && fishingManager != null)
        {
            if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
            {
                // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnMash","-> !MashPerformed");
                MashPerformed?.Invoke();
            }
            // else
            // {
            //     DebugLogger.Instance.Log("PlayerInputState: Not in reeling state, cannot perform mash action. Current fishing state: " + fishingManager.CurrentFishingGameState);
            // }
        }
    }

    public void OnReelLeft(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing)
        {
            if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
            {
                // Debug.Log("OnReelLeft called with value: " + value);

                ReelLeftPerformed?.Invoke();
            }

        }
    }

    public void OnReelRight(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing)
        {
            if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
            {
                // Debug.Log("OnReelRight called with value: " + value);

                ReelRightPerformed?.Invoke();
            }
        }
    }

    public void OnAbort(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing)
        {
            DebugLogger.Instance.LogMethodCall("PlayerInputState.OnAbort", "-> !AbortPerformed");
            AbortPerformed?.Invoke();
        }

    }

    public void OnDebug(InputValue value)
    {
        if (!value.isPressed) { return; }

        DebugLogger.Instance.LogMethodCall("PlayerInputState.OnDebug", "-> !DebugPerformed");
        DebugPerformed?.Invoke();
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