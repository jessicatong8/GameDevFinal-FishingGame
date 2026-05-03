using UnityEngine;

public class TensionManager : MonoBehaviour
{
    public bool IsInSafeZone() => tension >= safeZoneLower && tension <= safeZoneUpper;
    public bool IsTensionTooHigh() => tension > safeZoneUpper;
    public bool IsTensionTooLow() => tension < safeZoneLower;

    private Fish activeFish;
    private const float TARGET_MASH_RATE = 3f; // expected mashes per second
    private bool isReeling;
    private bool mashTriggeredThisFrame;
    // TENSION VARIABLES
    private float tension;
    private float maxTension = 100;
    private float tensionDropRate;
    // TENSION ZONES
    private float safeZoneLower;
    private float safeZoneUpper;
    // ESCAPE VARIABLES
    private float outOfZoneTimer; // counts how long tension has been out of the safe zone
    private float escapeTime; // if outOfZoneTimer exceeds escapeTime, fish escapes

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        PlayerInputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnCaught += HandleReset;
    }
    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        PlayerInputState.MashPerformed -= HandleMashPerformed;
        FishingManager.OnCaught -= HandleReset;
    }

    void Update()
    {
        if (!isReeling)
        {
            return;
        }

        UpdateTension(mashTriggeredThisFrame);
        UpdateEscapeTimer();
        mashTriggeredThisFrame = false;
        // DebugLogger.Instance.Log("Current Tension: " + tension);

    }

    private void HandleMashPerformed()
    {
        // DebugLogger.Instance.LogMethodCall("TensionManager: HandleMashPerformed");
        mashTriggeredThisFrame = true;
    }
    private void HandleHooked()
    {
        // DebugLogger.Instance.LogMethodCall("TensionManager: HandleHooked");
        activeFish = FishingManager.Instance.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("TensionManager: No active fish found.");
            return;
        }
        isReeling = true;
        tensionDropRate = activeFish.tensionDropRate;
        tension = activeFish.startingTension;

        safeZoneLower = activeFish.safeZoneCenter - activeFish.safeZoneWidth / 2f;
        safeZoneUpper = activeFish.safeZoneCenter + activeFish.safeZoneWidth / 2f;
        escapeTime = activeFish.tensionEscapeTime;

        outOfZoneTimer = 0f;
    }
    private void UpdateTension(bool mashedThisFrame)
    {
        if (mashedThisFrame)
        {
            float increment = activeFish.tensionIncreaseRate / TARGET_MASH_RATE;
            tension += increment;
            tension = Mathf.Min(tension, maxTension);
        }
        else
        {
            tension -= tensionDropRate * Time.deltaTime;
            tension = Mathf.Max(tension, 0);
        }
    }
    private void UpdateEscapeTimer()
    {
        if (IsInSafeZone())
        {
            outOfZoneTimer = 0f;
            return;
        }

        outOfZoneTimer += Time.deltaTime;

        if (outOfZoneTimer >= escapeTime)
        {
            outOfZoneTimer = 0f;
            FishingManager.Instance.InvokeEscaped("TensionOutOfRange");
        }
    }

    private void HandleReset()
    {
        mashTriggeredThisFrame = false;
        tension = 0f;
        outOfZoneTimer = 0f;
        isReeling = false;
        activeFish = null;
    }

    public float GetCurrentTension()
    {
        return tension;
    }

}
