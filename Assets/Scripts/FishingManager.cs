using UnityEngine;
using System;

public class FishingManager : MonoBehaviour
{
    //make events for future ui and audio 
    /**
    * Order of events during fishing sequence:
    * OnCast (on player pressing space to cast)
    * OnBite (player waits for random time -> gets bite)
    * OnHook (player has short window to press hook button to start reeling)
    * -- NoFishInSpot (if player tries to cast but there is no fish in the area)
    * -- DripTooLow (if player tries to cast but doesn't have enough "drip" to catch the fish in the area)
    * OnReeling (while player is reeling/mashing, progress and tension events will be fired to update UI)
        * Player will mash + arrow keys
        * CurrentProgressUpdated (float value between 0-100 indicating how close player is to catching the fish)
        * CurrentTensionUpdated (float value indicating current tension level of the line)
        * MaxTensionUpdated (float value indicating max tension level before line breaks - this is a property of the fish being caught)
    * OnCaught (if player successfully reels to 100% progress)
    * OnLineBreak (if tension reaches max tension)
    **/
    public int currentDrip; // will get from player or overarching score
    public static event Action OnCast;
    public static event Action OnBite;
    public static event Action OnHook;
    public static event Action NoFishInSpot;
    public static event Action DripTooLow; // when player doesn't have enough "drip" to catch the fish in the area
    public static event Action OnCaught;
    public static event Action OnLineBreak;
    public static event Action OnEscaped;
    public static event Action OnReturnToIdle;
    public static event Action<float> CurrentProgressUpdated; // for updating progress bar during reeling
    public static event Action<float> CurrentTensionUpdated; // for updating tension bar during reeling
    public static event Action<float> MaxTensionUpdated; // not used until multiple fish implemented
    public static event Action<FishingFlowState> OnFlowStateChanged; // for triggering state-specific animations
    public static event Action<Reeling_LineRangeState> OnReelLineRangeStateChanged; // for triggering line range related animations/effects during reeling
    public static event Action<Reeling_TensionState> OnReelTensionStateChanged; // for triggering tension related animations/effects during reeling
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

    public enum Reeling_TensionState
    {
        TooLow,
        Safe,
        TooHigh
    }

    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private bool bypassCastingManager = true;
    [SerializeField] private Fish fallbackFish;
    public CastingManager castingManager;
    public FishingFlowState CurrentFlowState => currentFlowState;
    public Reeling_LineRangeState CurrentReelLineRangeState => currentLineRangeState;
    public Reeling_TensionState CurrentReelTensionState => currentTensionState;

    // Starting Fishing States 
    private FishingFlowState currentFlowState = FishingFlowState.Idle;
    private Reeling_LineRangeState currentLineRangeState = Reeling_LineRangeState.InLineRange;
    private Reeling_TensionState currentTensionState = Reeling_TensionState.Safe;

    private float progress;
    private float tension;
    private float timer;    // general purpose timer used for casting and hook window states
    private float minHookDelay = 1.5f;
    private float maxHookDelay = 4f;
    private float minStableTensionRatio = 0.25f; // if tension is below 25% of max tension, we consider it too low
    private float maxStableTensionRatio = 0.75f; // if tension is above 75% of max tension, we consider it too high
    private float outOfRangeTimer; // timer to track how long player has been in too high/low tension state
    private float outOfRangeTimeLimit = 2f; // how long player can be in too high/low tension state before we consider them out of line range and trigger escape on next update tick
    private Fish activeFish;
    private bool mashTriggeredThisFrame;

    private void OnEnable()
    {
        if (inputState == null)
        {
            Debug.LogError("FishingManager: No PlayerInputState reference assigned in inspector.");
        }

        if (inputState != null)
        {
            inputState.HookPerformed += HandleHookPerformed;
            inputState.MashPerformed += HandleMashPerformed;
            inputState.AbortPerformed += HandleAbortPerformed;
        }
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.HookPerformed -= HandleHookPerformed;
            inputState.MashPerformed -= HandleMashPerformed;
            inputState.AbortPerformed -= HandleAbortPerformed;
        }
    }

    private void HandleHookPerformed()
    {
        Debug.Log("HandleHookPerformed called. Current input state: " + inputState.CurrentState);
        if (currentFlowState != FishingFlowState.HookWindow) { return; }
        Debug.Log("HandleHookPerformed check passed.");

        OnHook?.Invoke();
        EnterReelingState();
    }

    private void HandleMashPerformed()
    {
        if (currentFlowState == FishingFlowState.Reeling)
        {
            mashTriggeredThisFrame = true;
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
                TickReelingState(mashTriggeredThisFrame);
                break;
        }

        mashTriggeredThisFrame = false;
    }

    public bool TryStartFishing()
    {
        if (!IsPlayerOnDock() || currentFlowState != FishingFlowState.Idle)
        {
            Debug.Log("FishingManager: Cannot start fishing. IsPlayerOnDock: " + FishingAreaTrigger.IsPlayerInFishingArea + ", CurrentState: " + currentFlowState);
            return false;
        }

        return EnterCastingState();
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
            NoFishInSpot?.Invoke();
            return false;
        }

        if (currentDrip < activeFish.dripThreshold)
        {
            Debug.Log("Not enough drip to catch this fish! Current Drip: " + currentDrip + ", Required Drip: " + activeFish.dripThreshold);
            DripTooLow?.Invoke();
            return false;
        }

        OnCast?.Invoke();

        Debug.Log("Waiting for a bite/hook from fish: " + activeFish.fishName);
        SetFlowState(FishingFlowState.Casting);
        timer = UnityEngine.Random.Range(minHookDelay, maxHookDelay);
        return true;
    }

    private Fish ResolveFishForCurrentCast()
    {
        // Using CastingManager 
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

        progress = 0;
        tension = 0;
        outOfRangeTimer = 0;
        SetReelTensionState(Reeling_TensionState.Safe);
        SetReelLineRangeState(Reeling_LineRangeState.InLineRange);

        CurrentProgressUpdated?.Invoke(progress);
        CurrentTensionUpdated?.Invoke(tension);
        MaxTensionUpdated?.Invoke(activeFish.maxTension);
    }

    private void TickReelingState(bool mashedThisFrame)
    {
        if (mashedThisFrame)
        {
            progress += activeFish.reelingSpeed;
            progress = Mathf.Min(progress, 100f);

            tension += activeFish.reelingSpeed * 0.25f;
            tension = Mathf.Min(tension, activeFish.maxTension);

            CurrentProgressUpdated?.Invoke(progress);
            CurrentTensionUpdated?.Invoke(tension);
        }
        else
        {
            tension -= activeFish.tensionDropRate * Time.deltaTime;
            tension = Mathf.Max(tension, 0);
            CurrentTensionUpdated?.Invoke(tension);
        }

        UpdateReelingStateModel();

        if (progress >= 100)
        {
            OnCaught?.Invoke();
            ReturnToIdle("Fish caught.");
            return;
        }

        if (currentLineRangeState == Reeling_LineRangeState.OutOfLineRange)
        {
            if (currentTensionState == Reeling_TensionState.TooHigh)
            {
                OnLineBreak?.Invoke();
            }

            EscapeFishing("Fish escaped due to line range/tension state.");
        }
    }

    private void UpdateReelingStateModel()
    {
    Debug.Log($"NOT IMPLEMENTED: Updating reeling state. Progress: {progress}, Tension: {tension}");
    //     float maxTension = Mathf.Max(activeFish.maxTension, 0.0001f);
    //     float normalizedTension = Mathf.Clamp01(tension / maxTension);

    //     Reeling_TensionState nextTensionState;
    //     if (normalizedTension < minStableTensionRatio)
    //     {
    //         nextTensionState = Reeling_TensionState.TooLow;
    //     }
    //     else if (normalizedTension > maxStableTensionRatio)
    //     {
    //         nextTensionState = Reeling_TensionState.TooHigh;
    //     }
    //     else
    //     {
    //         nextTensionState = Reeling_TensionState.Safe;
    //     }
    //     SetReelTensionState(nextTensionState);

    //     Reeling_LineRangeState nextLineRangeState;
    //     if (nextTensionState == Reeling_TensionState.Safe)
    //     {
    //         outOfRangeTimer = 0f;
    //         nextLineRangeState = Reeling_LineRangeState.InLineRange;
    //     }
    //     else
    //     {
    //         // If tension is too high or too low, we start counting how long the player has been in this state. If they exceed the grace period, we consider them out of line range, which will lead to escape on next update tick.
    //         outOfRangeTimer += Time.deltaTime;
    //         nextLineRangeState = outOfRangeTimer >= outOfRangeTimeLimit
    //             ? Reeling_LineRangeState.OutOfLineRange
    //             : Reeling_LineRangeState.ExitingLineRange;
    //     }
    //     SetReelLineRangeState(nextLineRangeState);
    }

    private void SetFlowState(FishingFlowState requestedState)
    {
        if (currentFlowState == requestedState) { return; }

        currentFlowState = requestedState;
        OnFlowStateChanged?.Invoke(currentFlowState);
    }
    private void SetReelLineRangeState(Reeling_LineRangeState requestedState)
    {
        if (currentLineRangeState == requestedState) { return; }

        currentLineRangeState = requestedState;
        OnReelLineRangeStateChanged?.Invoke(currentLineRangeState);
    }
    private void SetReelTensionState(Reeling_TensionState requestedState)
    {
        if (currentTensionState == requestedState) { return; }

        currentTensionState = requestedState;
        OnReelTensionStateChanged?.Invoke(currentTensionState);
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
        progress = 0f;
        tension = 0f;
        timer = 0f;
        outOfRangeTimer = 0f;
        SetReelTensionState(Reeling_TensionState.Safe);
        SetReelLineRangeState(Reeling_LineRangeState.InLineRange);

        CurrentProgressUpdated?.Invoke(0f);
        CurrentTensionUpdated?.Invoke(0f);
        MaxTensionUpdated?.Invoke(0f);
        OnReturnToIdle?.Invoke();

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log("Fishing sequence ended: " + reason);
        }
    }

}