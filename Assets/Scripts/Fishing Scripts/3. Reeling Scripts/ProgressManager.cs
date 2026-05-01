using Unity.VisualScripting;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool mashTriggeredThisFrame;
    [SerializeField] private float startingProgress = 0f; // Can be set in inspector for testing different starting points.
    private float progress;   // 0-100 representing reeling progress towards catching the fish.
    private bool isReeling;
    Fish activeFish;

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        PlayerInputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnCaught += HandleResetToGameplay;
        // FishingManager.OnCatchConfirmationEnd += HandleResetToGameplay;
        FishingManager.OnEscaped += HandleResetToGameplay;
        FishingManager.OnReturnToGameplay += HandleResetToGameplay;
    }
    void Start()
    {
        HandleResetToGameplay();
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        PlayerInputState.MashPerformed -= HandleMashPerformed;
        FishingManager.OnCaught -= HandleResetToGameplay;
        // FishingManager.OnCatchConfirmationEnd -= HandleResetToGameplay;
        FishingManager.OnEscaped -= HandleResetToGameplay;
        FishingManager.OnReturnToGameplay -= HandleResetToGameplay;     // for aborts
    }
    private void HandleHooked()
    {
        activeFish = FishingManager.Instance.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("ProgressManager: No active fish found.");
        }
        isReeling = true;
    }
    private void HandleMashPerformed()
    {
        mashTriggeredThisFrame = true;
    }
    private void HandleResetToGameplay()
    {
        mashTriggeredThisFrame = false;
        progress = startingProgress;
        isReeling = false;
        activeFish = null;
    }
    private void UpdateProgress(bool mashedThisFrame)
    {
        if (mashedThisFrame && activeFish != null)
        {
            float increment = activeFish.reelingProgressRate / TARGET_MASH_RATE;
            progress += increment;
            progress = Mathf.Min(progress, 100f);
        }
        if (progress >= 100f)
        {
            FishingManager.Instance.CaughtFish();
        }
    }
    public float GetCurrentProgress()
    {
        return progress;
    }

    public bool IsProgressComplete()
    {
        return progress >= 100f;

    }
    void Update()
    {
        if (isReeling)
        {
            UpdateProgress(mashTriggeredThisFrame);
            mashTriggeredThisFrame = false;
        }
    }
}
