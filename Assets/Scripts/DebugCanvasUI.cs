using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Debug UI Canvas that displays all fishing states and method call logs
/// Attach this to a Canvas in the scene
/// </summary>
public class DebugCanvasUI : MonoBehaviour
{
    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private TextMeshProUGUI debugTextDisplay;

    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private float updateRate = 0.1f; // Update every 0.1 seconds
    [Header("Section Visibility")]
    [SerializeField] private bool showPlayerStateSection = true;
    [SerializeField] private bool showReelingSection = true;
    [SerializeField] private bool showActiveFishSection = true;
    [SerializeField] private bool showRecentCallsSection = true;
    [SerializeField] private int recentCallsToDisplay = 3;
    [SerializeField] private float recentCallsWindowSeconds = 5f;

    private float updateTimer;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        if (inputState == null)
            inputState = FindFirstObjectByType<PlayerInputState>();
        if (tensionManager == null)
            tensionManager = FindFirstObjectByType<TensionManager>();
        if (fishingManager == null)
            fishingManager = FindFirstObjectByType<FishingManager>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (debugTextDisplay == null)
        {
            DebugLogger.Instance.LogError("DebugCanvasUI: No TextMeshProUGUI assigned! Please assign the debug text display.");
            enabled = false;
            return;
        }

        if (inputState != null)
        {
            inputState.DebugPerformed += ToggleDebugUI;
        }
        else
        {
            DebugLogger.Instance.LogWarning("DebugCanvasUI: PlayerInputState not found!");
        }

        UpdateDebugDisplay();
    }

    private void OnDestroy()
    {
        if (inputState != null)
        {
            inputState.DebugPerformed -= ToggleDebugUI;
        }
    }

    private void ToggleDebugUI()
    {
        if (canvasGroup == null)
        {
            DebugLogger.Instance.LogError("DebugCanvasUI: CanvasGroup component missing!");
            return;
        }
        // Debug.Log($"Toggling Debug UI {showDebugUI} -> {!showDebugUI}");
        showDebugUI = !showDebugUI;

        Debug.Log($"Debug UI {(showDebugUI ? "shown" : "hidden")}");
        canvasGroup.alpha = showDebugUI ? 1f : 0f;
    }

    private void Update()
    {
        // Update display at regular intervals
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateRate)
        {
            UpdateDebugDisplay();
            updateTimer = 0f;
        }
    }

    private void UpdateDebugDisplay()
    {
        if (!showDebugUI || debugTextDisplay == null)
        {
            return;
        }

        string debugText = "";

        if (showPlayerStateSection)
        {
            // ===== PLAYER STATE =====
            debugText += "<b><color=#FFFF00>PLAYER STATE</color></b>\n";

            if (inputState != null)
            {
                debugText += $"Input State: <color=#00FFFF>{inputState.CurrentState}</color>\n";
            }
            else
            {
                debugText += "Input State: <color=#FF0000>PlayerInputState not found</color>\n";
            }
            debugText += $"Current Fishing Area: {(FishingAreaTrigger.IsPlayerInFishingArea ? "<color=#00FF00>Yes</color>" : "<color=#FF0000>No</color>")}\n";

            if (fishingManager != null)
            {
                debugText += $"Fishing State: <color=#00FFFF>{fishingManager.CurrentFishingGameState}</color>\n";
                debugText += $"Current Drip: {fishingManager.currentDrip}\n";
            }
            else
            {
                debugText += "<color=#FF0000>FishingManager not found!</color>\n";
            }

            debugText += "\n";
        }

        if (showReelingSection)
        {
            // ===== REELING STATE =====
            debugText += "<b><color=#FFFF00>REELING STATE</color></b>\n";

            bool isReeling = fishingManager != null && fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling;
            if (!isReeling)
            {
                debugText += "<color=#808080>Not reeling</color>\n";
                debugText += "\n";
            }
            else if (tensionManager != null && fishingManager != null)
            {
                double currentTension = tensionManager.GetCurrentTension();
                float maxTension = tensionManager.GetCurrentMaxTension();
                bool isInTensionSafeZone = tensionManager.IsInSafeZone();
                // bool isTensionTooHigh = tensionManager.IsTensionTooHigh();
                // bool isTensionTooLow = tensionManager.IsTensionTooLow();

                debugText += $"Tension: {currentTension:F2} / {maxTension:F2}\n";
                string tensionSafeZoneIndicator = isInTensionSafeZone ? "<color=#00FF00>SAFE</color>" : "<color=#FF0000>OUT OF ZONE</color>";
                debugText += $"Tension Zone?: {tensionSafeZoneIndicator}\n";
                // if (isTensionTooHigh)
                //     debugText += "<color=#FF0000>TENSION TOO HIGH!</color>\n";
                // else if (isTensionTooLow)
                //     debugText += "<color=#FFA500>TENSION TOO LOW!</color>\n";
                // else
                //     debugText += "<color=#00FF00>TENSION GOOD!</color>\n";

                debugText += $"Line?: <color=#00FFFF>{fishingManager.CurrentReelLineRangeState}</color>\n";
                debugText += "Wiggle?: <color=#808080>TODO</color>\n";
            }
            else
            {
                debugText += "<color=#FF0000>Required reeling references not found!</color>\n";
            }

            debugText += "\n";
        }

        if (showActiveFishSection)
        {
            // ===== ACTIVE FISH =====
            debugText += "<b><color=#FFFF00>ACTIVE FISH</color></b>\n";

            Fish activeFish = fishingManager != null ? fishingManager.activeFish : null;
            if (activeFish == null)
            {
                debugText += "<color=#808080>No active fish</color>\n";
            }
            else
            {
                debugText += $"Fish Name: {activeFish.fishName}\n";
                debugText += $"Object Name: {activeFish.name}\n";
                debugText += $"Swim Speed: {activeFish.swimmingSpeed:F2}\n";
                debugText += $"Rarity Weight: {activeFish.rarityWeight}\n";
                debugText += $"Rarity: {activeFish.rarity:F2}\n";
                debugText += $"Drip Threshold: {activeFish.dripThreshold:F2}\n";
                debugText += $"Wiggle On Timer: {activeFish.wiggleOnTimer:F2}\n";
                debugText += $"Wiggle Off Timer: {activeFish.wiggleOffTimer:F2}\n";
                debugText += $"Wiggle Strength: {activeFish.wiggleStrength:F2}\n";
                debugText += $"Max Tension: {activeFish.maxTension:F2}\n";
                debugText += $"Reeling Speed: {activeFish.reelingSpeed:F2}\n";
                debugText += $"Tension Drop Rate: {activeFish.tensionDropRate:F2}\n";
                debugText += $"Safe Zone Center: {activeFish.safeZoneCenter:F2}\n";
                debugText += $"Safe Zone Width: {activeFish.safeZoneWidth:F2}\n";
                debugText += $"Burst Strength: {activeFish.burstStrength:F2}\n";
                debugText += $"Tension Escape Time: {activeFish.tensionEscapeTime:F2}\n";
            }

            debugText += "\n";
        }

        if (showRecentCallsSection)
        {
            // ===== METHOD CALL LOG =====
            debugText += "<b><color=#FFFF00>RECENT CALLS</color></b>\n";

            float windowSeconds = Mathf.Max(0f, recentCallsWindowSeconds);
            var methodLog = DebugLogger.Instance.GetMethodLog(windowSeconds);
            if (methodLog.Count > 0)
            {
                // Display logs in reverse order (most recent first)
                // # of calls --> recentCallsToDisplay (default 3)
                int callsDisplayed = 0;
                int maxCalls = Mathf.Max(0, recentCallsToDisplay);
                for (int i = methodLog.Count - 1; i >= 0 && callsDisplayed < maxCalls; i--)
                {
                    debugText += $"- {methodLog[i]}\n";
                    callsDisplayed++;
                }
            }
            else
            {
                debugText += "<color=#808080>No method calls logged yet</color>\n";
            }

            debugText += "\n";
        }

        if (string.IsNullOrWhiteSpace(debugText))
        {
            debugText = "<color=#808080>All debug sections are hidden. Enable section toggles in the inspector.</color>";
        }

        debugTextDisplay.text = debugText;
    }
}
