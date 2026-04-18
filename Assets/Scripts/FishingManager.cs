using UnityEngine;
using System;

public class FishingManager : MonoBehaviour
{
    // This class manages the overall fishing game flow, including game state transitions, and event broadcasting for the fishing mini-game.

    public Fish activeFish;
    public TensionManager tensionManager;
    public ProgressManager progressManager;

    public int currentDrip; // will get from player or overarching score
    public static event Action OnCast; // when player initiates fishing by casting the line
    public static event Action OnBite; // after timer, when a fish bites the line and player has opportunity to hook
    public static event Action OnHook; // when player successfully hooks the fish by pressing the button within the hook window
    public static event Action NoFishInSpot;
    public static event Action DripTooLow; // when player doesn't have enough "drip" to catch the fish in the area
    public static event Action OnCaught; // when player successfully catches the fish by reeling to 100% progress
    public static event Action OnEscaped; // when fish escapes due to hook window timing out, player drip being too low, line breaking from high tension, or fish moving out of line range
    public static event Action OnReturnToIdle;

    public static event Action<FishingFlowState> OnFlowStateChanged; // for triggering state-specific animations
    public static event Action<Reeling_LineRangeState> OnReelLineRangeStateChanged; // for triggering line range related animations/effects during reeling
    public float hookTimer;

    public enum FishingFlowState
    {
        Idle,
        Casting,
        HookWindow,
        Reeling
    }

    public enum Reeling_LineRangeState
    {
        InLineRange,
        ExitingLineRange,
        OutOfLineRange
    }

    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private bool usePrototypeFishSequence = true;
    [SerializeField] private bool bypassCastingManager = true;
    [SerializeField] private Fish fallbackFish;
    // for prototyping, we preassign all fishes in the order they will be caught. In final version, we will use CastingManager to dynamically determine fish based on player's location and other factors
    [SerializeField] private Fish[] fishSequence;
    public CastingManager castingManager;
    public FishingFlowState CurrentFlowState => currentFlowState;
    public Reeling_LineRangeState CurrentReelLineRangeState => currentLineRangeState;

    // Starting Fishing States 
    private FishingFlowState currentFlowState = FishingFlowState.Idle;
    private Reeling_LineRangeState currentLineRangeState = Reeling_LineRangeState.InLineRange;

    private float timer;    // general purpose timer used for casting and hook window states
    private float minHookDelay = 1.5f;
    private float maxHookDelay = 4f;

    private float outOfRangeTimer; // timer to track how long player has been in too high/low tension state
    private float outOfRangeTimeLimit = 2f; // how long player can be in too high/low tension state before we consider them out of line range and trigger escape on next update tick
    private int fishSequenceIndex;


    void Start()
    {
        progressManager = GetComponent<ProgressManager>();
        tensionManager = GetComponent<TensionManager>();
    }

    private void OnEnable()
    {
        if (inputState == null)
        {
            Debug.LogError("FishingManager: No PlayerInputState reference assigned in inspector.");
        }

        if (inputState != null)
        {
            inputState.HookPerformed += HandleHookPerformed;
            // inputState.MashPerformed += HandleMashPerformed;
            inputState.AbortPerformed += HandleAbortPerformed;
        }
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.HookPerformed -= HandleHookPerformed;
            inputState.AbortPerformed -= HandleAbortPerformed;
        }
    }


    private void HandleHookPerformed()
    {
        Debug.Log("HandleHookPerformed called. Current input state: " + inputState.CurrentState);
        if (currentFlowState == FishingFlowState.HookWindow)
        {
            Debug.Log("HandleHookPerformed check passed.");

            OnHook?.Invoke();
            EnterReelingState();
        }
    }

    private void HandleAbortPerformed()
    {
        if (currentFlowState != FishingFlowState.Idle)
        {
            ReturnToIdle("Fishing aborted by player input.");
        }
    }

    private void Update()
    {
        switch (currentFlowState)
        {
            case FishingFlowState.Idle:
                break;

            case FishingFlowState.Casting:
                TickCastingState();
                break;

            case FishingFlowState.HookWindow:
                TickHookWindowState();
                break;

            case FishingFlowState.Reeling:
                HandleReeling();
                break;
        }

    }

    public bool TryStartFishing()
    {
        if (!IsPlayerOnDock() || currentFlowState != FishingFlowState.Idle)
        {
            Debug.Log("FishingManager: \nIsPlayerOnDock: " + IsPlayerOnDock() + "(should be true)" + "\nCurrentState: " + currentFlowState + "(should be Idle)");
            return false;
        }
        return EnterCastingState(); // returns true on success and false on failure (e.g. no fish in spot, drip too low)
    }

    private bool IsPlayerOnDock()
    {
        return FishingAreaTrigger.IsPlayerInFishingArea;
    }

    private bool EnterCastingState()
    {
        activeFish = ResolveFishForCurrentCast();
        if (activeFish == null)
        {
            Debug.Log("FishingManager: activeFish is null. Either no fish in the area to catch, or no fallback fish available!");
            NoFishInSpot?.Invoke();
            return false;
        }

        if (currentDrip < activeFish.dripThreshold)
        {
            Debug.Log("FishingManager: Not enough drip to catch this fish! Current Drip: " + currentDrip + ", Required Drip: " + activeFish.dripThreshold);
            DripTooLow?.Invoke();
            return false;
        }

        OnCast?.Invoke();

        Debug.Log("Fishing Manager: Entered casting state. Waiting for a bite from fish: " + activeFish.fishName);
        SetFlowState(FishingFlowState.Casting);
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
            Debug.LogWarning("FishingManager: Fish sequence is null or empty, but usePrototypeFishSequence is true. Please assign fish to the sequence in the inspector.");
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
            SetFlowState(FishingFlowState.HookWindow);
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
        SetFlowState(FishingFlowState.Reeling);

    }

    private void HandleReeling()
    {
        tensionManager = GetComponent<TensionManager>();
        progressManager = GetComponent<ProgressManager>();
        if (tensionManager.GetCurrentTension() > activeFish.maxTension)
        {
            EscapeFishing("Fish escaped due to line break from high tension.");
            return;
        }
        ;
        if (progressManager.GetCurrentProgress() >= 100f)
        {
            OnCaught?.Invoke();
            AdvanceFishSequenceOnCatch();
            ReturnToIdle(activeFish.fishName + " caught.");
            return;
        }
    }

    private void SetFlowState(FishingFlowState requestedState)
    {
        if (currentFlowState == requestedState) { return; }

        currentFlowState = requestedState;
        OnFlowStateChanged?.Invoke(currentFlowState);
    }


    private void EscapeFishing(string reason)
    {
        OnEscaped?.Invoke();
        ReturnToIdle(reason);
    }

    private void ReturnToIdle(string reason)
    {
        SetFlowState(FishingFlowState.Idle);

        activeFish = null;

        timer = 0f;
        outOfRangeTimer = 0f;

        OnReturnToIdle?.Invoke();

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log("Fishing sequence ended: " + reason);
        }
    }

}