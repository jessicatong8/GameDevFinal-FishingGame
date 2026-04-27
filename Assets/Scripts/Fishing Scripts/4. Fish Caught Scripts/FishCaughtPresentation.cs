using UnityEngine;

public class FishCaughtPresentation : MonoBehaviour
{
    [Header("Fish Placement")]
    // Optional transform to specify exact hold position relative to camera
    // [SerializeField] private float distanceFromCamera = 1f;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, 5f);
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Optional Freeze")]
    [SerializeField] private bool disableFishMovement = true;
    [SerializeField] private bool disableAnimator = true;
    [SerializeField] private bool freezeRigidbody = true;

    private FishMovement fishMovement;
    private Animator animator;
    private Rigidbody fishRigidbody;
    private bool isInCatchPresentation;

    private void Awake()
    {
        fishMovement = GetComponent<FishMovement>();
        animator = GetComponent<Animator>();
        fishRigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += FishCaughtPresentationAnimation;
        PlayerInputState.ConfirmCatchPerformed += HandleCatchConfirmation;
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= FishCaughtPresentationAnimation;
        PlayerInputState.ConfirmCatchPerformed -= HandleCatchConfirmation;
        RestoreFish();
    }

    private void FishCaughtPresentationAnimation()
    {
        PlaceFishInFrontOfCamera();
    }

    private void Update()
    {
        if (isInCatchPresentation)
        {
            RotateFish();
        }
    }

    private void HandleCatchConfirmation()
    {
        if (!isInCatchPresentation) { return; }

        isInCatchPresentation = false;
        FishingManager.Instance.CompleteCatchConfirmation();
        RestoreFish();
    }

    private void PlaceFishInFrontOfCamera()
    {
        if (disableFishMovement && fishMovement != null)
        {
            fishMovement.enabled = false;
        }

        if (disableAnimator && animator != null)
        {
            animator.enabled = false;
        }

        if (freezeRigidbody && fishRigidbody != null)
        {
            fishRigidbody.isKinematic = true;
            fishRigidbody.linearVelocity = Vector3.zero;
            fishRigidbody.angularVelocity = Vector3.zero;
        }

        Transform targetAnchor = PlayerCamera.Instance.transform;
        Vector3 targetPosition = targetAnchor.position + targetAnchor.forward;
        transform.position = targetPosition + targetAnchor.TransformVector(localOffset);
        transform.LookAt(PlayerCamera.Instance.transform.position);
        isInCatchPresentation = true;
    }
    private void RotateFish()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void RestoreFish()
    {
        DebugLogger.Instance.LogMethodCall("FishCaughtPresentation: RestoreFish");
        if (!isInCatchPresentation) { return; }
        DebugLogger.Instance.Log("Restoring fish to normal state.");
        if (freezeRigidbody && fishRigidbody != null)
        {
            fishRigidbody.isKinematic = false;
            fishRigidbody.detectCollisions = true;
        }

        if (disableFishMovement && fishMovement != null)
        {
            fishMovement.enabled = true;
        }

        if (disableAnimator && animator != null)
        {
            animator.enabled = true;
        }

        isInCatchPresentation = false;
    }
}