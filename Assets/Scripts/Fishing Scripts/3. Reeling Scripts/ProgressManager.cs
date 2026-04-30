using Unity.VisualScripting;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private bool autoCatchForTesting = false; // Set to true to automatically fill progress for testing purposes.
    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool mashTriggeredThisFrame;
    private float progress = 0f;
    private bool isReeling;

    Fish activeFish;

    void Start()
    {
        if (autoCatchForTesting) progress = 70f;
    }
    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        PlayerInputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnCaught += HandleResetToGameplay;
        // FishingManager.OnCatchConfirmationEnd += HandleResetToGameplay;
        FishingManager.OnEscaped += HandleResetToGameplay;
        FishingManager.OnReturnToGameplay += HandleResetToGameplay;
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
        progress = 0f;
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
