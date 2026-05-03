using UnityEngine;
using System;
using UnityEngine.SceneManagement;
// using Unity.XR.CoreUtils;

public class FishingManager : MonoBehaviour
{
    // Manage fishing gamestates and event broadcasting for the fishing.
    private static FishingManager instance;
    public static FishingManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<FishingManager>();
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts, such as TensionManager and ProgressManager during the reeling phase.


    public static event Action OnCast; // when player initiates fishing by casting the line
    public static event Action OnBite; // after timer when a fish bites line and player can try to hook
    public static event Action OnHook; // when player successfully hooks the fish within the hook window
    public static event Action OnEscaped; // when fish escapes due to hook window timing out, player drip being too low, line breaking from high tension, or fish moving out of line range
    public static event Action OnCaught; // when player successfully catches the fish by reeling to 100% progress, the fish will be presented to camera and player can interact to confirm catch (via !OnCatchConfirmationEnd) to return to idle
    public static event Action OnReturnToGameplay;

    public static event Action<FishingGameState> OnFishingGameStateChanged; // for triggering state-specific animations TODO: get rid of this, referenced in BasicUI

    public Fish activeFish;
    public string escapeReason;


    public enum FishingGameState
    {
        Gameplay, // default state when not fishing
        Casting, // After player initiates fishing by casting but before the fish bites
        HookWindow, // After fish bites and is within the window where player can hook, but hasn't successfully hooked yet
        Reeling, // After player successfully hooks and is trying to reel the fish in but hasn't caught the fish yet
        CatchPresentation // After player successfully reels to 100% progress and the fish is caught and presented to the camera, waiting for player to confirm catch before returning to idle
    }


    public FishingGameState CurrentFishingGameState => currentFishingGameState;
    private FishingGameState currentFishingGameState = FishingGameState.Gameplay;
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        PlayerInputState.AbortPerformed += HandleAbortPerformed;
    }
    private void OnDisable()
    {
        PlayerInputState.AbortPerformed -= HandleAbortPerformed;
    }

    private void SetFishingGameState(FishingGameState requestedState)
    {
        if (currentFishingGameState == requestedState) return;

        FishingGameState previousState = currentFishingGameState;
        currentFishingGameState = requestedState;
        // DebugLogger.Instance.LogMethodCall("FishingManager.SetFishingGameState", $"{previousState} -> {currentFishingGameState}");
        OnFishingGameStateChanged?.Invoke(currentFishingGameState);
    }
    public void InvokeCast()
    {
        SetFishingGameState(FishingGameState.Casting);
        OnCast?.Invoke();
    }
    public void InvokeBite()
    {
        SetFishingGameState(FishingGameState.HookWindow);
        OnBite?.Invoke();
    }

    public void InvokeHooked()
    {
        SetFishingGameState(FishingGameState.Reeling);
        OnHook?.Invoke();
    }
    private void HandleAbortPerformed()
    {
        if (currentFishingGameState != FishingGameState.Gameplay)
        {
            ReturnToGameplay("PlayerAborted");
        }
    }

    // Used in TensionManager, LineRangeManager, and CastingManager
    public void EscapeFishing(string reason)
    {
        // Debug.Log("Fish Escaped: " + escapeReason);
        DebugLogger.Instance.LogMethodCall("FishingManager.EscapeFishing", "-> !OnEscaped");
        OnEscaped?.Invoke();
        ReturnToGameplay(reason);
    }
    // Used in ProgressManager when player successfully reels in the fish
    public void CaughtFish()
    {
        string fishName = activeFish != null ? activeFish.fishName : "Unknown fish";
        DebugLogger.Instance.LogMethodCall("FishingManager.CaughtFish", fishName);
        SetFishingGameState(FishingGameState.CatchPresentation);
        OnCaught?.Invoke();
        // Wait for player to interact before advancing sequence and returning to idle
    }

    // All fishing outcomes (abort, escape, successful catch) resolve here to reset states and trigger animations
    public void ReturnToGameplay(string reason)
    {
        escapeReason = reason;
        if (currentFishingGameState == FishingGameState.Gameplay) return;
        DebugLogger.Instance.LogMethodCall("FishingManager.ReturnToGameplay", reason);

        SetFishingGameState(FishingGameState.Gameplay);

        if (activeFish != null)
        {
            activeFish.isActiveFish = false;
            activeFish = null;
        }
        else
        {
            // DebugLogger.Instance.LogWarning("FishingManager.ReturnToGameplay called with no active fish.");
        }
        OnReturnToGameplay?.Invoke();
    }

}