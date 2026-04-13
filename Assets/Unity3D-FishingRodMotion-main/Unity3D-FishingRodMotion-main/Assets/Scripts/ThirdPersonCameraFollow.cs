using UnityEngine;
using UnityEngine.InputSystem;

// Standard orbit-style third-person camera that follows a target.
public class ThirdPersonCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 1.8f;
    [SerializeField] private float followSmoothTime = 12f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 180f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Input System Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string lookActionName = "Look";

    private InputAction lookAction;
    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        if (target == null)
        {
            ThirdPController controller = FindFirstObjectByType<ThirdPController>();
            if (controller != null)
            {
                target = controller.transform;
            }
        }

        InitializeInputActions();

        Vector3 initialAngles = transform.eulerAngles;
        yaw = initialAngles.y;
        pitch = initialAngles.x;
    }

    private void OnEnable()
    {
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector2 lookInput = GetLookInput();

        yaw += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 lookTarget = target.position + Vector3.up * height;
        Vector3 desiredPosition = lookTarget + rotation * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothTime * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation((lookTarget - transform.position).normalized, Vector3.up);
    }

    private Vector2 GetLookInput()
    {
        if (lookAction != null)
        {
            return lookAction.ReadValue<Vector2>();
        }

        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        Vector2 gamepadLook = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() * 6f : Vector2.zero;
        return mouseDelta + gamepadLook;
    }

    private void InitializeInputActions()
    {
        if (inputActionsAsset == null)
        {
            PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
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

        lookAction = actionMap.FindAction(lookActionName, false);
        lookAction?.Enable();
    }
}
