using UnityEngine;

public class FishCaughtPresentation : MonoBehaviour
{
    [Header("Fish Placement")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, 2f);
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Optional Freeze")]
    [SerializeField] private bool disableFishMovement = true;
    [SerializeField] private bool disableAnimator = false;
    private FishMovement fishMovement;
    private Animator animator;
    private bool isInCatchPresentation;
    private bool catchPresentationApplied;

    private void Awake()
    {
        fishMovement = GetComponent<FishMovement>();
        animator = GetComponent<Animator>();
        if (fishMovement == null || animator == null)
        {
            DebugLogger.Instance.LogWarning("FishCaughtPresentation: Missing components. FishMovement: " + fishMovement + ", Animator: " + animator);
        }
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

        RestoreFish();
        FishingManager.Instance.CompleteCatchConfirmation();
    }

    private void PlaceFishInFrontOfCamera()
    {
        if (disableFishMovement) { fishMovement.enabled = false; }
        if (disableAnimator) { animator.enabled = false; }

        Transform targetAnchor = PlayerCamera.Instance.transform;
        Vector3 targetPosition = targetAnchor.position + targetAnchor.forward;
        transform.position = targetPosition + targetAnchor.TransformVector(localOffset);
        transform.LookAt(PlayerCamera.Instance.transform.position);
        isInCatchPresentation = true;
        catchPresentationApplied = true;
    }

    private void RotateFish()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void RestoreFish()
    {
        if (!catchPresentationApplied)
        {
            DebugLogger.Instance.LogWarning("FishCaughtPresentation: RestoreFish called but catch presentation was not applied. Skipping restore.");
            return;
        }
        // DebugLogger.Instance.LogMethodCall("FishCaughtPresentation: RestoreFish");
        // DebugLogger.Instance.Log("Restoring fish to normal state.");

        if (disableFishMovement && fishMovement != null)
        {
            fishMovement.enabled = true;
        }

        if (disableAnimator && animator != null)
        {
            animator.enabled = true;
        }

        isInCatchPresentation = false;
        catchPresentationApplied = false;
    }
}
