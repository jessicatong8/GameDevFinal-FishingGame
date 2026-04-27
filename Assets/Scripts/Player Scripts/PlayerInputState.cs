using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerFishing))]
public class PlayerInputState : MonoBehaviour
{
    public enum InputStates
    {
        Gameplay,
        Fishing
    }
    [SerializeField] private InputStates currentState = InputStates.Gameplay;

    public Vector2 MovementInputData { get; private set; }
    public Vector2 LookInputData { get; private set; }
    public float ZoomInputData { get; private set; }

    public event Action InteractPerformed;
    public event Action JumpPerformed;
    public event Action HookPerformed;
    public event Action MashPerformed;

    public event Action ReelLeftPerformed;
    public event Action ReelRightPerformed;

    public event Action AbortPerformed;
    public event Action DebugPerformed;


    public InputStates CurrentState => currentState;
    [SerializeField] private FishingManager fishingManager;
    private PlayerMovement movementScript;
    // private PlayerFishing fishingScript;


    public void Awake()
    {
        // DebugLogger.Instance.Log($"PlayerInputState initialized with state: {currentState}");
        movementScript = GetComponent<PlayerMovement>();
        // fishingScript = GetComponent<PlayerFishing>();
        // menuScript = GetComponent<PlayerMenu>();
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
            // Call interact for PlayerFishing, which will check if fishing can be started and if so, will switch to fishing state. 
            DebugLogger.Instance.LogMethodCall("PlayerInputState.OnInteract", "-> !InteractPerformed");
            InteractPerformed?.Invoke();
        }
    }

    // public void OnJump(InputValue value)
    // {
    //     if (!value.isPressed) { return; }

    //     if (currentState == InputStates.Gameplay)
    //     {
    //         // DebugLogger.Instance.LogMethodCall("PlayerInputState.OnJump","-> !JumpPerformed");
    //         JumpPerformed?.Invoke();
    //     }
    // }

    public void OnHook(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Fishing)
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

        if (currentState == InputStates.Fishing)
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