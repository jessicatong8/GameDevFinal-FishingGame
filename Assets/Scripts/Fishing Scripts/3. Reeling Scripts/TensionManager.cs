using UnityEngine;

public class TensionManager : MonoBehaviour
{
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

    void Start()
    {
        tension = 0f;
    }
    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        PlayerInputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnCaught += HandleCaught;
        // FishingManager.OnCaught += HandleResetToIdle;
        // FishingManager.OnCatchConfirmationEnd += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
        FishingManager.OnReturnToIdle += HandleResetToIdle;
    }
    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        PlayerInputState.MashPerformed -= HandleMashPerformed;
        FishingManager.OnCaught -= HandleCaught;
        // FishingManager.OnCaught -= HandleResetToIdle;
        // FishingManager.OnCatchConfirmationEnd += HandleResetToIdle;
        FishingManager.OnEscaped -= HandleResetToIdle;
        FishingManager.OnReturnToIdle -= HandleResetToIdle;
    }
    private void HandleMashPerformed()
    {
        // DebugLogger.Instance.LogMethodCall("TensionManager: HandleMashPerformed");
        mashTriggeredThisFrame = true;
    }
    void Update()
    {
        if (isReeling)
        {
            UpdateTension(mashTriggeredThisFrame);
            UpdateEscapeTimer();
            mashTriggeredThisFrame = false;
            // DebugLogger.Instance.Log("Current Tension: " + tension);
        }
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
        tension = activeFish.safeZoneCenter * maxTension; // start in the middle of the safe zone

        safeZoneLower = maxTension * (activeFish.safeZoneCenter - activeFish.safeZoneWidth / 2f);
        safeZoneUpper = maxTension * (activeFish.safeZoneCenter + activeFish.safeZoneWidth / 2f);
        escapeTime = activeFish.tensionEscapeTime;

        outOfZoneTimer = 0f;
    }

    private void HandleCaught()
    {
        mashTriggeredThisFrame = false;
        isReeling = false;
    }

    private void UpdateTension(bool mashedThisFrame)
    {
        if (mashedThisFrame)
        {

            float increment = activeFish.reelingSpeed / TARGET_MASH_RATE;
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
            FishingManager.Instance.EscapeFishing("Tension too high, line snapped");
        }
    }


    private void HandleResetToIdle()
    {
        // DebugLogger.Instance.LogMethodCall("TensionManager: HandleResetToIdle");
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
    public bool IsInSafeZone() => tension >= safeZoneLower && tension <= safeZoneUpper;
    public bool IsTensionTooHigh() => tension > safeZoneUpper;
    public bool IsTensionTooLow() => tension < safeZoneLower;


}
