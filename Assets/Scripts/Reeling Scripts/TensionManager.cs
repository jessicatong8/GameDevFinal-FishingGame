using UnityEngine;

public class TensionManager : MonoBehaviour
{
    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private FishingManager fishingManager;
    Fish activeFish;
    private const float TARGET_MASH_RATE = 3f; // expected mashes per second


    private bool isReeling;
    private bool mashTriggeredThisFrame;
    private float tension;
    private float maxTension;
    private float tensionDropRate;

    private float safeZoneLower;
    private float safeZoneUpper;
    private float escapeTime; // how many seconds you have of exceeding safe tension before the fish escapes
    private float outOfZoneTimer;



    private void OnEnable()
    {
        inputState.MashPerformed += HandleMashPerformed;
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
        FishingManager.OnReturnToIdle += HandleResetToIdle;
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.MashPerformed -= HandleMashPerformed;
        }

        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleResetToIdle;
        FishingManager.OnEscaped -= HandleResetToIdle;
        FishingManager.OnReturnToIdle -= HandleResetToIdle;
    }

    private void HandleMashPerformed()
    {
        // DebugLogger.Instance.LogMethodCall("TensionManager: HandleMashPerformed");
        mashTriggeredThisFrame = true;
    }
    void Start()
    {
        tension = 0f;
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
        activeFish = fishingManager.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("TensionManager: No active fish found.");
            return;
        }
        isReeling = true;
        maxTension = activeFish.maxTension;
        tensionDropRate = activeFish.tensionDropRate;
        tension = activeFish.safeZoneCenter * maxTension; // start in the middle of the safe zone

        safeZoneLower = maxTension * (activeFish.safeZoneCenter - activeFish.safeZoneWidth / 2f);
        safeZoneUpper = maxTension * (activeFish.safeZoneCenter + activeFish.safeZoneWidth / 2f);
        escapeTime = activeFish.tensionEscapeTime;

        outOfZoneTimer = 0f;


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
            // DebugLogger.Instance.Log("Fish Escaped - Tension too high for too long");
            fishingManager.EscapeFishing("Tension too high, line snapped");
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
    public float GetCurrentMaxTension()
    {
        return maxTension;
    }
    public bool IsInSafeZone() => tension >= safeZoneLower && tension <= safeZoneUpper;
    public bool IsTensionTooHigh() => tension > safeZoneUpper;
    public bool IsTensionTooLow() => tension < safeZoneLower;


}
