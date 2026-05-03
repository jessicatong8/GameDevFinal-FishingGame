using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private float startingProgress = 0f; // Can be set in inspector for testing different starting points.

    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool mashTriggeredThisFrame;
    private float progress;   // 0-100 representing reeling progress towards catching the fish.
    private bool isReeling = false;
    Fish activeFish;

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        // FishingManager.OnCaught += HandleReset;
        PlayerInputState.MashPerformed += HandleMashPerformed;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        // FishingManager.OnCaught -= HandleReset;
        PlayerInputState.MashPerformed -= HandleMashPerformed;
    }
    void Update()
    {
        if (isReeling)
        {
            UpdateProgress(mashTriggeredThisFrame);
            mashTriggeredThisFrame = false;
        }
    }

    private void HandleHooked()
    {
        activeFish = FishingManager.Instance.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("ProgressManager: No active fish found.");
        }
        isReeling = true;
        progress = startingProgress;

    }
    private void HandleMashPerformed()
    {
        mashTriggeredThisFrame = true;
    }
    private void HandleReset()
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
            FishingManager.Instance.InvokeCaught();
            HandleReset();
        }
    }
    public float GetCurrentProgress()
    {
        return progress;
    }
}
