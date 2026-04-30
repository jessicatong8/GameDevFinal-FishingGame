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
    private PlayerInputState inputState;
    private TensionManager tensionManager;
    private FishingManager fishingManager;
    private GameObject debugPanel;
    private TextMeshProUGUI debugTextDisplay;

    [SerializeField] private bool showDebugUI = false;
    [SerializeField] private float updateRate = 0.1f; // Update every 0.1 seconds
    [Header("Section Visibility")]
    [SerializeField] private bool showPlayerStateSection = true;
    [SerializeField] private bool showFishingSection = true;
    [SerializeField] private bool showActiveFishSection = true;
    [SerializeField] private bool showRecentCallsSection = true;
    [SerializeField] private int recentCallsToDisplay = 3;
    [SerializeField] private float recentCallsWindowSeconds = 3f;
    private float updateTimer;
    private void Start()
    {
        inputState = PlayerInputState.Instance;
        fishingManager = FishingManager.Instance;
        tensionManager = FindFirstObjectByType<TensionManager>();
        debugPanel = transform.Find("Debug Panel").gameObject;
        if (debugPanel == null)
        {
            DebugLogger.Instance.LogError("DebugCanvasUI: No Image component found on the GameObject!");
        }

        debugTextDisplay = GetComponentInChildren<TextMeshProUGUI>(true);
        if (debugTextDisplay == null)
        {
            DebugLogger.Instance.LogError("DebugCanvasUI: No TextMeshProUGUI assigned!");
        }
        PlayerInputState.DebugPerformed += ToggleDebugUI;
        UpdateDebugDisplay();
    }

    private void OnDestroy()
    {
        if (inputState != null)
        {
            PlayerInputState.DebugPerformed -= ToggleDebugUI;
        }
    }

    private void ToggleDebugUI()
    {
        // Debug.Log($"Toggling Debug UI {showDebugUI} -> {!showDebugUI}");
        showDebugUI = !showDebugUI;
        Debug.Log($"Debug UI {(showDebugUI ? "shown" : "hidden")}");
        debugPanel.SetActive(showDebugUI);
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
        if (!showDebugUI || debugTextDisplay == null) return;

        string debugText = "";
        if (showPlayerStateSection)
        {
            // ===== PLAYER STATE =====
            // Input State
            debugText += "<b><color=#FFFF00>PLAYER STATE</color></b>\n";
            debugText += $"Input State: <color=#00FFFF>{inputState.CurrentState}</color>\n";
            // Fishing Area
            debugText += $"Current Fishing Area: {(FishingAreaTrigger.IsPlayerInFishingArea ? "<color=#00FF00>Yes</color>" : "<color=#FF0000>No</color>")}\n";
            // Player's Fishing Status
            debugText += $"Fishing State: <color=#00FFFF>{fishingManager.CurrentFishingGameState}</color>\n";
            debugText += $"Current Drip: {fishingManager.currentDrip}\n";

            debugText += "\n";
        }

        if (showFishingSection)
        {
            // ===== FISHING STATUS =====
            debugText += "<b><color=#FFFF00>FISHING STATUS</color></b>\n";
            bool isFishing = fishingManager.CurrentFishingGameState != FishingManager.FishingGameState.Gameplay;
            bool isReeling = fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.Reeling;
            bool isInCatchPresentation = fishingManager != null && fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation;

            if (!isFishing && !isInCatchPresentation)
            {
                debugText += "<color=#808080>Not fishing</color>\n";
                debugText += "\n";
            }
            else if (tensionManager != null)
            {
                if (isFishing)
                {
                    // if (isReeling)
                    // {
                    //     debugText += "<color=#00FF00>Currently Reeling!</color>\n";
                    // }
                    // TENSION
                    double currentTension = tensionManager.GetCurrentTension();
                    float maxTension = 100f;
                    bool isInTensionSafeZone = tensionManager.IsInSafeZone();

                    debugText += $"Tension: {currentTension:F2} / {maxTension:F2}\n";
                    string tensionSafeZoneIndicator = isInTensionSafeZone ? "<color=#00FF00>SAFE</color>" : "<color=#FF0000>OUT OF ZONE</color>";
                    debugText += $"Tension Zone?: {tensionSafeZoneIndicator}\n";

                    // LINE RANGE
                    debugText += $"Line?: <color=#00FFFF>{(LineRangeManager.Instance.isInLineRange ? "IN RANGE" : "OUT OF RANGE")}</color>\n";

                    // WIGGLE
                    debugText += "Wiggle?: <color=#808080>TODO</color>\n";
                    debugText += "\n";
                }
                if (isInCatchPresentation)
                {
                    debugText += "InCatchPresentation: " + (isInCatchPresentation ? "<color=#00FF00>Yes</color>" : "<color=#FF0000>No</color>") + "\n";
                }
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
                debugText += $"Drip Level: {activeFish.level:F2}\n";

                // debugText += $"Wiggle On Timer: {activeFish.wiggleOnTimer:F2}\n";
                // debugText += $"Wiggle Off Timer: {activeFish.wiggleOffTimer:F2}\n";
                // debugText += $"Wiggle Strength: {activeFish.wiggleStrength:F2}\n";
                // debugText += $"Max Tension: {activeFish.maxTension:F2}\n";

                // TENSION
                // debugText += $"Reeling Speed: {activeFish.reelingSpeed:F2}\n";
                debugText += $"Tension Drop Rate: {activeFish.tensionDropRate:F2}\n";
                debugText += $"Safe Zone Lower: {activeFish.safeZoneCenter - activeFish.safeZoneWidth / 2:F2}\n";
                debugText += $"Safe Zone Upper: {activeFish.safeZoneCenter + activeFish.safeZoneWidth / 2:F2}\n";
                debugText += $"Tension Escape Time: {activeFish.tensionEscapeTime:F2}\n";

                // LINE RANGE
                debugText += $"IsInLineRange?: {(LineRangeManager.Instance.isInLineRange ? "<color=#00FF00>Yes</color>" : "<color=#FF0000>No</color>")}\n";

                // ACTIVE FISH
                debugText += $"Is Active Fish?: {(activeFish.isActiveFish ? "<color=#00FF00>Yes</color>" : "<color=#FF0000>No</color>")}\n";
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
