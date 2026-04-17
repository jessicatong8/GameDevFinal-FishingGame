using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
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
    public event Action ReelPerformed;
    public event Action MashPerformed;
    public event Action AbortPerformed;

    public InputStates CurrentState => currentState;

    public void Awake()
    {
        Debug.Log($"PlayerInputState initialized with state: {currentState}");
    }
    public void SetState(InputStates state)
    {
        Debug.Log($"Input state changed to: {state}");
        currentState = state;
        ClearInputs();
    }

    public void OnMove(InputValue value)
    {
        if (currentState != InputStates.Gameplay)
        {
            return;
        }

        MovementInputData = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (currentState != InputStates.Gameplay && currentState != InputStates.Fishing)
        {
            return;
        }

        LookInputData = value.Get<Vector2>();
    }

    public void OnZoomScroll(InputValue value)
    {
        // if (currentState == InputStates.Menu) { return; }
        ZoomInputData = value.Get<Vector2>().y;
    }

    public void OnZoom(InputValue value)
    {
        OnZoomScroll(value);
    }

    public void OnZoomToggle(InputValue value)
    {
        // if (currentState == InputStates.Menu) { return; }
        if (value.isPressed)
        {
            // Toggle zoom between 1st person and 3rd person
            ZoomInputData = ZoomInputData == 0f ? 1f : 0f;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
        {
            // Debug.Log("Interact input ignored. Current state: " + currentState + ", Value pressed: " + value.isPressed);
            return;
        }
        if (currentState == InputStates.Gameplay)
        {
            Debug.Log("Fishing Started");
            InteractPerformed?.Invoke();
        }
        if (currentState == InputStates.Fishing)
        {
            Debug.Log("Casting Started");
            ReelPerformed?.Invoke();
        }
    }

    public void OnJump(InputValue value)
    {
        if (currentState != InputStates.Gameplay || !value.isPressed)
        {
            return;
        }
        JumpPerformed?.Invoke();
    }

    public void OnReel(InputValue value)
    {
        if (currentState != InputStates.Fishing || !value.isPressed)
        {
            Debug.Log("PlayerInputState: Reel input ignored. Current state: " + currentState + ", Value pressed: " + value.isPressed);
            return;
        }

        ReelPerformed?.Invoke();
    }

    public void OnMash(InputValue value)
    {
        if (currentState != InputStates.Fishing || !value.isPressed)
        {
            Debug.Log("PlayerInputState: Mash input ignored. Current state: " + currentState + ", Value pressed: " + value.isPressed);
            return;
        }

        MashPerformed?.Invoke();
    }

    public void OnAbort(InputValue value)
    {
        if (currentState != InputStates.Fishing || !value.isPressed)
        {
            Debug.Log("PlayerInputState: Abort input ignored. Current state: " + currentState + ", Value pressed: " + value.isPressed);
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