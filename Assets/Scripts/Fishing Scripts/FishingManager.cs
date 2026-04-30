using UnityEngine;
using System;
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
    public static event Action NoFishInSpot;
    public static event Action DripTooLow; // when player doesn't have enough "drip" to catch the next fish
    public static event Action OnEscaped; // when fish escapes due to hook window timing out, player drip being too low, line breaking from high tension, or fish moving out of line range
    public static event Action OnCaught; // when player successfully catches the fish by reeling to 100% progress, the fish will be presented to camera and player can interact to confirm catch (via !OnCatchConfirmationEnd) to return to idle
    // public static event Action OnCatchConfirmationEnd; // when player confirms catch by pressing interact or clicking, which will trigger !onReturnToGameplay to reset everything for the next catch
    public static event Action OnReturnToGameplay;

    public static event Action<FishingGameState> OnFishingGameStateChanged; // for triggering state-specific animations

    public Fish activeFish;
    public int currentDrip; // will get from player or overarching score

    public enum FishingGameState
    {
        Gameplay, // default state when not fishing
        Casting,
        HookWindow,
        Reeling,
        CatchPresentation
    }
    // public CastingManager castingManager;
    public FishingGameState CurrentFishingGameState => currentFishingGameState;
    private FishingGameState currentFishingGameState = FishingGameState.Gameplay;
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        PlayerInputState.HookPerformed += InvokeHooked;
        PlayerInputState.AbortPerformed += HandleAbortPerformed;
    }
    private void OnDisable()
    {
        PlayerInputState.HookPerformed -= InvokeHooked;
        PlayerInputState.AbortPerformed -= HandleAbortPerformed;
    }
    private void EnterReelingState()
    {
        SetFishingGameState(FishingGameState.Reeling);

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
    private void HandleAbortPerformed()
    {
        if (currentFishingGameState != FishingGameState.Gameplay)
        {
            ReturnToGameplay("Fishing aborted by player input.");
        }
    }
    public void InvokeHooked()
    {
        DebugLogger.Instance.Log("HandleHookPerformed called. Current input state: " + PlayerInputState.Instance.CurrentState);
        // if (currentFishingGameState == FishingGameState.HookWindow)
        {
            // DebugLogger.Instance.Log("HandleHookPerformed check passed.");
            DebugLogger.Instance.LogMethodCall("FishingManager.HandleHookPerformed", "-> !OnHook\nHook successful");
            OnHook?.Invoke();
            EnterReelingState();
        }
    }
    public void EscapeFishing(string reason)
    {
        DebugLogger.Instance.LogMethodCall("FishingManager.EscapeFishing", "-> !OnEscaped");
        OnEscaped?.Invoke();
        ReturnToGameplay(reason);
    }
    public void CaughtFish()
    {
        string fishName = activeFish != null ? activeFish.fishName : "Unknown fish";
        DebugLogger.Instance.LogMethodCall("FishingManager.CaughtFish", fishName);
        SetFishingGameState(FishingGameState.CatchPresentation);
        OnCaught?.Invoke();
        // Wait for player to interact before advancing sequence and returning to idle
    }
    public void CompleteCatchConfirmation()
    {
        // DebugLogger.Instance.LogMethodCall("FishingManager.CompleteCatchConfirmation", "");
        // OnCatchConfirmationEnd?.Invoke();
        // AdvanceFishSequenceOnCatch();
        // string fishName = activeFish != null ? activeFish.fishName : "Unknown fish";
        // ReturnToIdle(fishName + " caught.");
    }
    // All fishing outcomes (abort, escape, successful catch) resolve here to reset states and trigger animations
    public void ReturnToGameplay(string reason)
    {
        DebugLogger.Instance.LogMethodCall("FishingManager.ReturnToGameplay", reason);

        SetFishingGameState(FishingGameState.Gameplay);
        if (activeFish != null)
        {
            activeFish.isActiveFish = false;
            activeFish = null;
        }
        else
        {
            DebugLogger.Instance.LogWarning("FishingManager.ReturnToGameplay called with no active fish.");
        }
        OnReturnToGameplay?.Invoke();
    }
}