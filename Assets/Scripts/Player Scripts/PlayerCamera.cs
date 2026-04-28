using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{    public static PlayerCamera Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PlayerCamera>();
            }
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts.
    
    [Header("References")]
    public Transform target;
    public LayerMask environmentLayerMask;

    [Header("Camera Orbit (Exploration)")]
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
    public float firstPersonThreshold = 0.1f;

    [Header("Fishing Mode Settings")]
    public float fishingHeightOffset = 2.5f;
    public float fishingPitch = 40f;
    public float fishingDistance = 4f;
    public float transitionSpeed = 5f; // How fast the camera snaps to fishing mode

    [Header("Dock Alignment")]
    public Transform dockTransform; // Assign the "Dock" object here
    public float alignmentSmoothTime = 0.2f;
    private float rotationVelocity;

    private static PlayerCamera instance; 
    private float pitch;
    private float yaw;
    private float distance;
    private float currentDistance;
    private float currentHeightOffset;
    private float currentPitch;

    [Header("Collision")]
    // public float cameraCollisionRadius = 0.3f;
    // public float cameraCollisionBuffer = 0.2f;
    public float cameraCollisionRadius = 0.2f;
    public float cameraCollisionBuffer = 0.05f;

    public void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        pitch = 20f; // Start at a slight angle
        yaw = target.eulerAngles.y;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        currentDistance = distance;
        currentHeightOffset = targetHeightOffset;
    }

    private void Update()
    {
        if (PlayerInputState.Instance == null)
        {
            DebugLogger.Instance.LogError("PlayerCamera: PlayerInputState.Instance is null. Make sure PlayerInputState is in the scene.");
            return;
        }

        bool isFishing = PlayerInputState.Instance.GetCurrentInputState() == PlayerInputState.InputStates.Fishing;

        if (isFishing)
        {
            // 1. Calculate the target Yaw from the dock/fishing spot
            // We use LookRotation to find the angle on the Y axis
            Vector3 dockForward = dockTransform.forward;
            float targetYaw = Mathf.Atan2(dockForward.x, dockForward.z) * Mathf.Rad2Deg;

            // 2. Smoothly rotate the camera's yaw to face the dock forward
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref rotationVelocity, alignmentSmoothTime);

            // 3. Force the Player to face the same direction
            // This ensures the 3rd person model is fishing toward the water
            target.rotation = Quaternion.Euler(0, yaw, 0);

            // Set static fishing targets
            currentDistance = Mathf.Lerp(currentDistance, fishingDistance, Time.deltaTime * transitionSpeed);
            currentHeightOffset = Mathf.Lerp(currentHeightOffset, fishingHeightOffset, Time.deltaTime * transitionSpeed);
            currentPitch = Mathf.Lerp(currentPitch, fishingPitch, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Standard Orbit Controls (Active when NOT fishing)
            Vector2 lookInput = PlayerInputState.Instance.LookInputData;
            yaw += lookInput.x * yawSensitivity;
            pitch -= lookInput.y * pitchSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            float zoomInput = PlayerInputState.Instance.ZoomInputData;
            distance = Mathf.Clamp(distance - scrollZoomSpeed * zoomInput * Time.deltaTime, minDistance, maxDistance);

            // Return to standard heights
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * transitionSpeed);


            currentHeightOffset = Mathf.Lerp(currentHeightOffset, targetHeightOffset, Time.deltaTime * transitionSpeed);
            currentPitch = Mathf.Lerp(currentPitch, pitch, Time.deltaTime * transitionSpeed);
        }

        UpdateCameraTransform(currentPitch, currentHeightOffset, currentDistance);
    }

    private void UpdateCameraTransform(float activePitch, float activeHeight, float activeDist)
    {
        // 1. Calculate Rotation and Focus Point
        Quaternion rotation = Quaternion.Euler(activePitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * activeHeight;

        // 2. Handle Camera Collision
        float finalDistance = activeDist;

        // Only perform collision check if we are outside the first-person threshold
        if (finalDistance > firstPersonThreshold)
        {
            Vector3 direction = -(rotation * Vector3.forward);

            // SphereCast from the focus point outwards to the desired camera position
            if (Physics.SphereCast(
                    focusPoint,
                    cameraCollisionRadius,
                    direction,
                    out RaycastHit hit,
                    activeDist,
                    environmentLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                // If we hit something, pull the camera forward
                // The buffer prevents the camera from clipping into the geometry
                finalDistance = Mathf.Max(hit.distance - cameraCollisionBuffer, firstPersonThreshold);
            }
        }

        // 3. Finalize Position and Rotation
        Vector3 cameraPosition = focusPoint - (rotation * Vector3.forward) * finalDistance;
        transform.SetPositionAndRotation(cameraPosition, rotation);

        // 4. Handle 1st Person Visibility (Optional Hint)
        // If finalDistance is very small, you might want to hide the player mesh here
    }
}
