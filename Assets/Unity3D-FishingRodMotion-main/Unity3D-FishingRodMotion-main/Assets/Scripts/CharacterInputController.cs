using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputController : MonoBehaviour {

    private float filteredForwardInput = 0f;
    private float filteredTurnInput = 0f;

    public bool InputMapToCircular = true;

    public float forwardInputFilter = 5f;
    public float turnInputFilter = 5f;

    private float forwardSpeedLimit = 1f;

    [Header("Input System Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";

    private InputAction moveAction;
    private InputAction jumpAction;


    public float Forward
    {
        get;
        private set;
    }

    public float Turn
    {
        get;
        private set;
    }

    public bool Action
    {
        get;
        private set;
    }

    public bool Jump
    {
        get;
        private set;
    }

        

    void Start()
    {
        InitializeInputActions();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    void Update () {
        float h = 0f;
        float v = 0f;

        if (moveAction != null)
        {
            Vector2 actionMove = moveAction.ReadValue<Vector2>();
            h = actionMove.x;
            v = actionMove.y;
        }
        else
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            }

            Vector2 keyboardMove = Vector2.ClampMagnitude(new Vector2(h, v), 1f);
            Vector2 gamepadMove = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
            Vector2 move = Vector2.ClampMagnitude(keyboardMove + gamepadMove, 1f);

            h = move.x;
            v = move.y;
        }

        if (InputMapToCircular)
        {
            // make coordinates circular
            // based on http://mathproofs.blogspot.com/2005/07/mapping-square-to-circle.html
            h = h * Mathf.Sqrt(1f - 0.5f * v * v);
            v = v * Mathf.Sqrt(1f - 0.5f * h * h);
        }

        // do some filtering of our input as well as clamp to a speed limit
        filteredForwardInput = Mathf.Clamp(Mathf.Lerp(filteredForwardInput, v, 
            Time.deltaTime * forwardInputFilter), -forwardSpeedLimit, forwardSpeedLimit);

        filteredTurnInput = Mathf.Lerp(filteredTurnInput, h, 
            Time.deltaTime * turnInputFilter);

        Forward = filteredForwardInput;
        Turn = filteredTurnInput;

        if (jumpAction != null)
        {
            Jump = jumpAction.WasPressedThisFrame();
        }
        else
        {
            Jump = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        }
    }

    private void InitializeInputActions()
    {
        if (inputActionsAsset == null)
        {
            var playerInput = GetComponent<PlayerInput>();
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

        EnableInputActions();
    }

    private void EnableInputActions()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    private void DisableInputActions()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }
}