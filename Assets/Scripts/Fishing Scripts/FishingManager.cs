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
            if (instance == null)
            {
                instance = FindFirstObjectByType<FishingManager>();
            }
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
    // public static event Action OnCatchConfirmationEnd; // when player confirms catch by pressing interact or clicking, which will trigger !onReturnToIdle to reset everything for the next catch
    public static event Action OnReturnToIdle;

    public static event Action<FishingGameState> OnFishingGameStateChanged; // for triggering state-specific animations

    public Fish activeFish;
    public int currentDrip; // will get from player or overarching score
    public float hookTimer;
    public enum FishingGameState
    {
        Idle,
        Casting,
        HookWindow,
        Reeling,
        CatchPresentation
    }


    public CastingManager castingManager;
    public FishingGameState CurrentFishingGameState => currentFishingGameState;

    [SerializeField] private bool usePrototypeFishSequence = true;
    [SerializeField] private bool bypassCastingManager = true;
    [SerializeField] private Fish fallbackFish;
    // for prototyping, we preassign all fishes in the order they will be caught. In final version, we will use CastingManager to dynamically determine fish based on player's location and other factors
    [SerializeField] private Fish[] fishSequence;
    // Starting Fishing States 
    private FishingGameState currentFishingGameState = FishingGameState.Idle;

    private float timer;    // general purpose timer used for casting and hook window states
    private float minHookDelay = 1.5f;
    private float maxHookDelay = 4f;
    private int fishSequenceIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        PlayerInputState.HookPerformed += HandleHookPerformed;
        PlayerInputState.AbortPerformed += HandleAbortPerformed;
    }

    private void OnDisable()
    {
        PlayerInputState.HookPerformed -= HandleHookPerformed;
        PlayerInputState.AbortPerformed -= HandleAbortPerformed;
    }

    private bool IsPlayerInFishingArea()
    {
        return FishingAreaTrigger.IsPlayerInFishingArea;
    }

    public bool TryStartFishing()
    {
        // DebugLogger.Instance.LogMethodCall("FishingManager.TryStartFishing");
        if (!IsPlayerInFishingArea() || currentFishingGameState != FishingGameState.Idle)
        {
            DebugLogger.Instance.LogMethodCall("FishingManager.TryStartFishing", "IsPlayerInFishingArea: " + IsPlayerInFishingArea() + " (should be true)" + "\nCurrentState: " + currentFishingGameState + " (should be Idle)");
            return false;
        }
        return EnterCastingState(); // returns true on success and false on failure (e.g. no fish in spot, drip too low)
    }

    private void HandleHookPerformed()
    {
        DebugLogger.Instance.Log("HandleHookPerformed called. Current input state: " + PlayerInputState.Instance.CurrentState);
        if (currentFishingGameState == FishingGameState.HookWindow)
        {
            // DebugLogger.Instance.Log("HandleHookPerformed check passed.");
            DebugLogger.Instance.LogMethodCall("FishingManager.HandleHookPerformed", "-> !OnHook\nHook successful");
            OnHook?.Invoke();
            EnterReelingState();
        }
    }

    private void HandleAbortPerformed()
    {
        if (currentFishingGameState != FishingGameState.Idle)
        {
            ReturnToIdle("Fishing aborted by player input.");
        }
    }

    private bool EnterCastingState()
    {
        activeFish = ResolveFishForCurrentCast();
        if (activeFish == null)
        {
            DebugLogger.Instance.Log("FishingManager: activeFish is null. Either no fish in the area to catch, or no fallback fish available!");
            NoFishInSpot?.Invoke();
            return false;
        }

        if (currentDrip < activeFish.dripThreshold)
        {
            DebugLogger.Instance.Log("FishingManager: Not enough drip to catch this fish! Current Drip: " + currentDrip + ", Required Drip: " + activeFish.dripThreshold);
            DripTooLow?.Invoke();
            return false;
        }
        activeFish.isActiveFish = true;

        // DebugLogger.Instance.LogMethodCall("FishingManager.EnterCastingState", "-> !OnCast\nCasting line with fish: " + activeFish.fishName);
        OnCast?.Invoke();

        SetFishingGameState(FishingGameState.Casting);
        timer = UnityEngine.Random.Range(minHookDelay, maxHookDelay);
        return true;
    }

    private Fish ResolveFishForCurrentCast()
    {
        // For prototyping purposes, use a preassigned sequence of fish that will be caught in order.
        if (usePrototypeFishSequence)
        {
            Fish sequenceFish = GetFishFromSequence();
            if (sequenceFish != null)
            {
                return sequenceFish;
            }
        }

        // Using CastingManager, checks collision area for fish and returns fish if found.
        // TODO - does this only check once when first entering casting state, or should we check continuously while in casting state to wait for a fish to swim into the area?
        if (!bypassCastingManager && castingManager != null)
        {
            Fish detectedFish = castingManager.GetFishInArea();
            if (detectedFish != null)
            {
                return detectedFish;
            }
        }

        // Bypassing CastingManager
        if (fallbackFish != null)
        {
            return fallbackFish;
        }

        return null;
    }

    // Prototyping Function ONLY
    private Fish GetFishFromSequence()
    {
        if (fishSequence == null || fishSequence.Length == 0)
        {
            DebugLogger.Instance.LogWarning("FishingManager: Fish sequence is null or empty, but usePrototypeFishSequence is true. Please assign fish to the sequence in the inspector.");
            return null;
        }

        if (fishSequenceIndex < 0)
        {
            fishSequenceIndex = 0;
        }

        while (fishSequenceIndex < fishSequence.Length)
        {
            Fish nextFish = fishSequence[fishSequenceIndex];
            if (nextFish != null)
            {
                return nextFish;
            }

            // Skip empty slots so prototyping arrays can have gaps.
            fishSequenceIndex++;
        }

        // if no valid fish within entire sequence, return null.
        return null;
    }

    // Prototyping Function ONLY
    private void AdvanceFishSequenceOnCatch()
    {
        if (!usePrototypeFishSequence || fishSequence == null || fishSequence.Length == 0)
        {
            return;
        }

        if (fishSequenceIndex < fishSequence.Length)
        {
            fishSequenceIndex++;
        }
    }

    private void TickCastingState()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            OnBite?.Invoke();
            SetFishingGameState(FishingGameState.HookWindow);
            timer = hookTimer;
        }
    }

    private void TickHookWindowState()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EscapeFishing("Fish escaped because hook window timed out.");
        }
    }

    private void EnterReelingState()
    {
        SetFishingGameState(FishingGameState.Reeling);

    }

    // private void HandleReeling()
    // {
    // tensionManager = GetComponent<TensionManager>();
    // progressManager = GetComponent<ProgressManager>();
    // if (tensionManager.GetCurrentTension() > activeFish.maxTension)
    // {
    //     EscapeFishing("Fish escaped due to line break from high tension.");
    //     return;
    // }
    // ;
    // if (progressManager.IsProgressComplete())
    // {
    //     OnCaught?.Invoke();
    //     AdvanceFishSequenceOnCatch();
    //     ReturnToIdle(activeFish.fishName + " caught.");
    //     return;
    // }
    // }

    private void SetFishingGameState(FishingGameState requestedState)
    {
        if (currentFishingGameState == requestedState) { return; }

        FishingGameState previousState = currentFishingGameState;
        currentFishingGameState = requestedState;
        DebugLogger.Instance.LogMethodCall("FishingManager.SetFishingGameState", $"{previousState} -> {currentFishingGameState}");
        OnFishingGameStateChanged?.Invoke(currentFishingGameState);
    }


    public void EscapeFishing(string reason)
    {
        DebugLogger.Instance.LogMethodCall("FishingManager.EscapeFishing", "-> !OnEscaped");
        OnEscaped?.Invoke();
        ReturnToIdle(reason);
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
        AdvanceFishSequenceOnCatch();
        string fishName = activeFish != null ? activeFish.fishName : "Unknown fish";
        ReturnToIdle(fishName + " caught.");
    }

    public void ReturnToIdle(string reason)
    {
        DebugLogger.Instance.LogMethodCall("FishingManager.ReturnToIdle", reason);

        SetFishingGameState(FishingGameState.Idle);
        activeFish.isActiveFish = false;
        activeFish = null;
        timer = 0f;
        PlayerAnimator.Instance.animator.SetBool("isReeling", false);
        PlayerAnimator.Instance.animator.SetTrigger("stopFishing");

        OnReturnToIdle?.Invoke();

        if (!string.IsNullOrEmpty(reason))
        {
            DebugLogger.Instance.Log("Fishing sequence ended: " + reason);
        }
    }

    private void Update()
    {
        switch (currentFishingGameState)
        {
            case FishingGameState.Idle:
                break;

            case FishingGameState.Casting:
                TickCastingState();
                break;

            case FishingGameState.HookWindow:
                TickHookWindowState();
                break;

            case FishingGameState.Reeling:
                // HandleReeling(); // Now handled by ProgressManager and TensionManager
                break;
        }
    }
}