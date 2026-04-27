using Unity.VisualScripting;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{

    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool mashTriggeredThisFrame;
    private float progress = 0f; // TODO SET TO 0f after testing
    private bool isReeling;

    Fish activeFish;

    void Start()
    {
        progress = 0f; // TODO SET TO 0f after testing
    }
    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        PlayerInputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnCatchConfirmationEnd += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
        FishingManager.OnReturnToIdle += HandleResetToIdle;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        PlayerInputState.MashPerformed -= HandleMashPerformed;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnCatchConfirmationEnd -= HandleResetToIdle;
        FishingManager.OnEscaped -= HandleResetToIdle;
        FishingManager.OnReturnToIdle -= HandleResetToIdle;
    }
    private void HandleHooked()
    {
        activeFish = FishingManager.Instance.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("ProgressManager: No active fish found.");
        }
        // progress = 0f;
        isReeling = true;
    }
    private void HandleCaught()
    {
        mashTriggeredThisFrame = false;
        isReeling = false;
    }
    private void HandleMashPerformed()
    {
        mashTriggeredThisFrame = true;
    }
    private void HandleResetToIdle()
    {
        mashTriggeredThisFrame = false;
        progress = 0f;     // TODO SET TO 0f after testing
        isReeling = false;
        activeFish = null;
    }
    private void UpdateProgress(bool mashedThisFrame)
    {
            if (mashedThisFrame && activeFish != null)
        {
            float increment = activeFish.reelingSpeed / TARGET_MASH_RATE;
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
