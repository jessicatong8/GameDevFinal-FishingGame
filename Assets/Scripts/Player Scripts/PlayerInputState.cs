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
    public event Action AbortPerformed;

    public InputStates CurrentState => currentState;
    [SerializeField] private FishingManager fishingManager;
    private PlayerMovement movementScript;
    private PlayerFishing fishingScript;


    public void Awake()
    {
        Debug.Log($"PlayerInputState initialized with state: {currentState}");
        movementScript = GetComponent<PlayerMovement>();
        fishingScript = GetComponent<PlayerFishing>();
        // menuScript = GetComponent<PlayerMenu>();
    }
    public void SetState(InputStates state)
    {
        Debug.Log($"PlayerInputState: Switching to state: {currentState}");
        currentState = state;
        ClearInputs();
        switch (currentState)
        {
            case InputStates.Gameplay:
                movementScript.enabled = true;
                fishingScript.enabled = false;
                break;
            case InputStates.Fishing:
                movementScript.enabled = false;
                fishingScript.enabled = true;
                break;
            default:
                Debug.LogWarning("PlayerInputState: Unhandled state: " + currentState);
                break;
        }
    }
    public InputStates GetCurrentInputState()
    {
        return currentState;
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

    public void OnZoomScroll(InputValue value)
    {
        // if (currentState != InputStates.Menu) { return; }
        ZoomInputData = value.Get<Vector2>().y;
    }

    public void OnZoom(InputValue value)
    {
        if (currentState == InputStates.Gameplay)
        {
            OnZoomScroll(value);
        }
    }

    public void OnZoomToggle(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Gameplay)
        {
            // Toggle zoom between 1st person and 3rd person
            ZoomInputData = ZoomInputData == 0f ? 1f : 0f;
        }
    }

    public void OnInteract(InputValue value)
    {
        // For one off actions
        if (!value.isPressed) { return; }

        if (currentState == InputStates.Gameplay)
        {
            Debug.Log("PlayerInputState: Interact in Gameplay state.");
            // Call interact for Playerfishing, which will check if fishing can be started and if so, will switch to fishing state. 
            InteractPerformed?.Invoke();
            return;
        }
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState != InputStates.Gameplay)
        {
            // Debug.Log("Wrong state for jump input. Current state: " + currentState);
            return;
        }
        JumpPerformed?.Invoke();
    }

    public void OnHook(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState != InputStates.Fishing)
        {
            // Debug.Log("Wrong state for hook input. Current state: " + currentState);
            return;
        }
        HookPerformed?.Invoke();
    }

    public void OnMash(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState != InputStates.Fishing)
        {
            // Debug.Log("Wrong state for mash input. Current state: " + currentState);
            return;
        }
        MashPerformed?.Invoke();
    }

    public void OnAbort(InputValue value)
    {
        if (!value.isPressed) { return; }

        if (currentState != InputStates.Fishing)
        {
            // Debug.Log("Wrong state for abort input. Current state: " + currentState);
            return;
        }

        AbortPerformed?.Invoke();
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