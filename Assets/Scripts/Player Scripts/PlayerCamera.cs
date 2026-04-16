using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

// based on https://discussions.unity.com/t/roblox-like-camera-script/211199/2

[RequireComponent(typeof(Camera))]

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]

    // Target is the main point the camera orbits around (the player's head/eyes)
    public Transform target;
    // Focus is the point the camera looks at, which can be offset from the target for better framing
    public Transform cameraFocus;
    public LayerMask environmentLayerMask;
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string lookActionName = "Look";

    [Header("Camera Orbit")]
    public float targetHeightOffset = 1.6f;
    public float minPitch = -75f;
    public float maxPitch = 80f;
    public float yawSensitivity = 0.18f;
    public float pitchSensitivity = 0.18f;

    [Header("Zoom - 1st Person + 3rd Person")]
    public float startDistance = 6f;
    public float minDistance = 0f;
    public float maxDistance = 12f;
    public float scrollZoomSpeed = 5f;
    public float zoomSmoothTime = 0.05f;
    public float firstPersonThreshold = 0.1f;

    [Header("Collision")]
    public float cameraCollisionRadius = 0.2f;
    public float cameraCollisionBuffer = 0.05f;

    [Header("Starting Orientation")]
    public float startPitch = 20f;
    public float startYaw = 0f;

    private float pitch;
    private float yaw;
    private float distance;
    private float tempDistance;
    private float currentDistance;
    private float zoomVelocity;
    private InputAction lookAction;
    private PlayerInput playerInput;
    private bool usesPlayerInputMaps;

    void Start()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<Player>()?.transform;
        }

        InitializeInputActions();

        pitch = startPitch;
        yaw = startYaw;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (XRSettings.isDeviceActive)
        {
            return;
        }

        ApplyLookInput();
        ApplyZoomInput();
        UpdateCameraTransform();
    }

    private void ApplyLookInput()
    {
        if (usesPlayerInputMaps
            && playerInput != null
            && (playerInput.currentActionMap == null || playerInput.currentActionMap.name != playerActionMapName))
        {
            return;
        }

        Vector2 lookInput;
        if (lookAction != null)
        {
            lookInput = lookAction.ReadValue<Vector2>();
        }
        else
        {
            Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            Vector2 gamepadLook = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() * 15f * Time.deltaTime : Vector2.zero;
            lookInput = mouseDelta + gamepadLook;
        }

        yaw += lookInput.x * yawSensitivity;
        pitch -= lookInput.y * pitchSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void ApplyZoomInput()
    {
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            // Scroll up zooms in (toward first person), scroll down zooms out.
            distance = Mathf.Clamp(distance - scrollZoomSpeed * scroll * Time.deltaTime, minDistance, maxDistance);
        }

        distance = Mathf.SmoothDamp(distance, tempDistance, ref zoomVelocity, zoomSmoothTime);
    }

    private void UpdateCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = GetFocusPoint();

        float finalDistance = Mathf.Max(currentDistance, 0f);

        if (finalDistance > firstPersonThreshold)
        {
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward) * finalDistance;
            Vector3 castDirection = (desiredPosition - focusPoint).normalized;

            if (Physics.SphereCast(
                focusPoint,
                cameraCollisionRadius,
                castDirection,
                out RaycastHit hit,
                finalDistance,
                environmentLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                finalDistance = Mathf.Max(hit.distance - cameraCollisionBuffer, firstPersonThreshold);
            }
        }

        Vector3 cameraPosition = focusPoint - (rotation * Vector3.forward) * finalDistance;
        transform.SetPositionAndRotation(cameraPosition, rotation);
    }

    private Vector3 GetFocusPoint()
    {
        if (cameraFocus != null)
        {
            return cameraFocus.position;
        }

        Transform namedFocus = target.Find("Eyes");
        if (namedFocus == null)
        {
            namedFocus = target.Find("CameraFocus");
        }

        if (namedFocus != null)
        {
            return namedFocus.position;
        }

        return target.position + Vector3.up * targetHeightOffset;
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

        InputActionMap actionMap = inputActionsAsset.FindActionMap(playerActionMapName, false);
        if (actionMap == null)
        {
            return;
        }

        lookAction = actionMap.FindAction(lookActionName, false);
        if (!usesPlayerInputMaps)
        {
            lookAction?.Enable();
        }
    }
}
