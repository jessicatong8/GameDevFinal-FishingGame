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
    public static event Action OnCast;
    public static event Action OnBite;
    public static event Action OnHook;
    public static event Action NoFishInSpot;
    public static event Action DripTooLow; // when player doesn't have enough "drip" to catch the fish in the area
    public static event Action OnCaught;
    public static event Action OnLineBreak;
    public static event Action OnReturnToIdle;
    public static event Action<float> CurrentProgressUpdated; // for updating progress bar during reeling
    public static event Action<float> CurrentTensionUpdated; // for updating tension bar during reeling
    public static event Action<float> MaxTensionUpdated; // not used until multiple fish implemented
    public float hookTimer;

    public enum FishingState
    {
        Idle,
        Waiting,
        HookWindow,
        Reeling
    }
    [SerializeField] private PlayerInputState inputState;
    [SerializeField] private bool bypassCastingManager = true;
    [SerializeField] private Fish fallbackFish;
    public CastingManager castingManager;

    private FishingState currentState = FishingState.Idle;
    private float progress;
    private float tension;
    private float timer;
    private Fish activeFish;
    public int currentDrip; // will get from player or overarching score
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
        if (currentState != FishingState.HookWindow) { return; }

        OnHook?.Invoke();
        StartReeling();
    }

    private void HandleMashPerformed()
    {
        if (currentState == FishingState.Reeling)
        {
            mashTriggeredThisFrame = true;
        }
    }

    private void HandleAbortPerformed()
    {
        if (currentState != FishingState.Idle)
        {
            AbortFishing();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case FishingState.Idle:
                break;

            case FishingState.Waiting:
                HandleWaiting();
                break;

            case FishingState.HookWindow:
                HandleHookWindow();
                break;

            case FishingState.Reeling:
                HandleReeling(mashTriggeredThisFrame);
                break;
        }

        mashTriggeredThisFrame = false;
    }

    // 
    public bool TryStartFishing()
    {
        if (!IsPlayerOnDock() || currentState != FishingState.Idle)
        {
            Debug.Log("FishingManager: Cannot start fishing. IsPlayerOnDock: " + FishingAreaTrigger.IsPlayerInFishingArea + ", CurrentState: " + currentState);
            return false;
        }
        
        return StartWaiting();
    }

    private bool IsPlayerOnDock()
    {
        return FishingAreaTrigger.IsPlayerInFishingArea;
    }

    bool StartWaiting()
    {
        activeFish = CastingManagerBypassFish();
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

        Debug.Log("Currently waiting to get a bite from the fish: " + activeFish.fishName);
        currentState = FishingState.Waiting;
        timer = UnityEngine.Random.Range(2f, 5f);
        return true;
    }

    private Fish CastingManagerBypassFish()
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

    void HandleWaiting()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            OnBite?.Invoke();

            currentState = FishingState.HookWindow;

            timer = hookTimer;
        }
    }

    void HandleHookWindow()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (!StartWaiting())
            {
                AbortFishing();
            }
        }
    }

    void StartReeling()
    {
        currentState = FishingState.Reeling;
        progress = 0;
        tension = 0;
        CurrentProgressUpdated?.Invoke(progress);
        CurrentTensionUpdated?.Invoke(tension);
        MaxTensionUpdated?.Invoke(activeFish.maxTension);
    }

    void HandleReeling(bool mashedThisFrame)
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


        if (progress >= 100)
        {
            OnCaught?.Invoke();
            AbortFishing();
        }
        if (tension >= activeFish.maxTension)
        {
            OnLineBreak?.Invoke();
            AbortFishing();
        }
    }

    void AbortFishing()
    {
        currentState = FishingState.Idle;
        activeFish = null;
        progress = 0f;
        tension = 0f;
        timer = 0f;
        CurrentProgressUpdated?.Invoke(0f);
        CurrentTensionUpdated?.Invoke(0f);
        MaxTensionUpdated?.Invoke(0f);
        OnReturnToIdle?.Invoke();
        Debug.Log("Fishing sequence aborted. Returning to idle state.");
    }

}