using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public LayerMask environmentLayerMask;
    [SerializeField] private PlayerInputState inputState;

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
    private float currentDistance;
    private float zoomVelocity;

    private void Awake()
    {
        if (inputState == null)
        {
            inputState = GetComponentInParent<PlayerInputState>();
        }
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("PlayerCamera: No target assigned. Please assign a target Transform for the camera to orbit around.");
            enabled = false;
            return;
        }

        if (XRSettings.isDeviceActive)
        {
            enabled = false;
            return;
        }

        pitch = startPitch;
        yaw = startYaw;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        currentDistance = distance;
    }

    private void Update()
    {
        if (inputState != null)
        {
            Vector2 lookInput = inputState.LookInputData;
            yaw += lookInput.x * yawSensitivity;
            pitch -= lookInput.y * pitchSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            float zoomInput = inputState.ZoomInputData;
            if (Mathf.Abs(zoomInput) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scrollZoomSpeed * zoomInput * Time.deltaTime, minDistance, maxDistance);
            }
        }

        // currentDistance = Mathf.SmoothDamp(currentDistance, distance, ref zoomVelocity, zoomSmoothTime);
        currentDistance = distance;
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * targetHeightOffset;

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
}