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
    public static event Action OnLevelUp; // when player confirms catch and levels up, the level up presentation is shown and player must confirm before returning to gameplay
    public static event Action OnReturnToGameplay;
    public static event Action OnGameWin; // after a catch is confirmed, when player catches all fish and wins the game
    public Fish activeFish;
    public string ReturnToGameplayReason;
    // private bool playerLeveledUpFromLastCatch = false; // tracks if player leveled up from the most recent catch confirmation


    public enum FishingGameState
    {
        Gameplay, // default state when not fishing
        Casting, // After player initiates fishing by casting but before the fish bites
        HookWindow, // After fish bites and is within the window where player can hook, but hasn't successfully hooked yet
        Reeling, // After player successfully hooks and is trying to reel the fish in but hasn't caught the fish yet
        CatchPresentation, // After player successfully reels to 100% progress and the fish is caught and presented to the camera, waiting for player to confirm catch before returning to idle
        LevelUpPresentation // After player confirms catch and levels up, waiting for player to acknowledge level up before returning to idle
    }
    public FishingGameState CurrentFishingGameState => currentFishingGameState;
    private FishingGameState currentFishingGameState = FishingGameState.Gameplay;
    private void Awake()
    {
        Instance = this;
    }
    private void SetFishingGameState(FishingGameState requestedState)
    {
        if (currentFishingGameState == requestedState) return;

        FishingGameState previousState = currentFishingGameState;
        currentFishingGameState = requestedState;
        // DebugLogger.Instance.LogMethodCall("FishingManager.SetFishingGameState", $"{previousState} -> {currentFishingGameState}");
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
    public void InvokeGameWin()
    {
        OnGameWin?.Invoke();
    }

    // Used in TensionManager, LineRangeManager, and CastingManager
    public void EscapeFishing(string reason)
    {
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
        // playerLeveledUpFromLastCatch = false; // reset for new catch
        OnCaught?.Invoke();
        // Wait for player to interact before advancing sequence and returning to idle
    }

    // Called when player confirms a catch and has leveled up; transitions to LevelUpPresentation state
    public void TransitionToLevelUpPresentation()
    {
        SetFishingGameState(FishingGameState.LevelUpPresentation);
        // playerLeveledUpFromLastCatch = true;
        OnLevelUp?.Invoke();
    }

    // Called when player confirms the level up presentation; returns to idle gameplay
    public void ConfirmLevelUpPresentation()
    {
        DebugLogger.Instance.LogMethodCall("FishingManager.ConfirmLevelUpPresentation", "LevelUpPresentation -> Gameplay");
        ReturnToGameplay("LevelUpConfirmed");
    }


    // All fishing outcomes (abort, escape, successful catch) resolve here to reset states and trigger animations
    public void ReturnToGameplay(string reason)
    {
        if (currentFishingGameState == FishingGameState.Gameplay) return;
        ReturnToGameplayReason = reason;
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