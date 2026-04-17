using UnityEngine;
using System;

public class FishingManager : MonoBehaviour
{
    //make events for future ui and audio 
    /**
    * Order of events during fishing sequence:
    * OnCast (on player pressing space to cast)
    * OnBite (player waits for random time -> gets bite)
    * OnReelAttempt (player has short window to press reel button to start reeling)
    * NoFishInSpot (if player tries to cast but there is no fish in the area)
    * DripTooLow (if player tries to cast but doesn't have enough "drip" to catch the fish in the area)
    * OnCaught (if player successfully reels to 100% progress)
    * OnLineBreak (if tension reaches max tension)
    **/
    public static event Action OnCast;
    public static event Action OnBite;
    public static event Action OnReelAttempt;
    public static event Action NoFishInSpot;
    public static event Action DripTooLow; // when player doesn't have enough "drip" to catch the fish in the area
    public static event Action OnCaught;
    public static event Action OnLineBreak;
    public static event Action OnReturnToIdle;
    public static event Action<float> CurrentProgressUpdated; // for updating progress bar during reeling
    public static event Action<float> CurrentTensionUpdated; // for updating tension bar during reeling
    public static event Action<float> MaxTensionUpdated; // 
    public float hookTimer;
    
    public enum FishingState
    {
        Idle,
        Waiting,
        HookWindow,
        Reeling
    }
    [SerializeField] private PlayerInputState inputState;
    [Header("Fish Selection")]
    [SerializeField] private bool bypassCastingManager = true;
    [SerializeField] private Fish fallbackFish;
    public CastingManager castingManager;

    private FishingState currentState = FishingState.Idle;
    private float progress;
    private float tension;
    private float timer;
    private Fish activeFish;
    public int currentDrip; // will get from player or overarching score
    private bool isPlayerOnDock = false;
    private bool mashTriggeredThisFrame;

    private void OnEnable()
    {
        FishingAreaTrigger.OnPlayerEnterFishingArea += SetFishingAreaStatus;
        FishingAreaTrigger.OnPlayerExitFishingArea += SetFishingAreaStatus;

        if (inputState == null)
        {
            Debug.LogError("FishingManager: No PlayerInputState reference assigned in inspector.");
        }

        if (inputState != null)
        {
            inputState.ReelPerformed += HandleReelPerformed;
            inputState.MashPerformed += HandleMashPerformed;
            inputState.AbortPerformed += HandleAbortPerformed;
        }
    }

    private void OnDisable()
    {
        FishingAreaTrigger.OnPlayerEnterFishingArea -= SetFishingAreaStatus;
        FishingAreaTrigger.OnPlayerExitFishingArea -= SetFishingAreaStatus;

        if (inputState != null)
        {
            inputState.ReelPerformed -= HandleReelPerformed;
            inputState.MashPerformed -= HandleMashPerformed;
            inputState.AbortPerformed -= HandleAbortPerformed;
        }
    }

    private void HandleReelPerformed()
    {
        if (currentState != FishingState.HookWindow)
        {
            return;
        }

        OnReelAttempt?.Invoke();
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

    private void SetFishingAreaStatus(bool isInFishingArea){
        isPlayerOnDock = isInFishingArea;
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

    public bool TryStartFishing()
    {
        if (!IsPlayerOnDock() || currentState != FishingState.Idle)
        {
            Debug.Log("Cannot start fishing. IsPlayerOnDock: " + isPlayerOnDock + ", CurrentState: " + currentState);
            return false;
        }

        return StartWaiting();
    }

    private bool IsPlayerOnDock()
    {
        if (FishingAreaTrigger.IsPlayerInFishingArea)
        {
            isPlayerOnDock = true;
            return true;
        }

        if (isPlayerOnDock)
        {
            return true;
        }

        return false;
    }

    bool StartWaiting()
    {
        activeFish = ResolveFishForCast();
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

        currentState = FishingState.Waiting;
        timer = UnityEngine.Random.Range(2f, 5f);
        return true;
    }

    private Fish ResolveFishForCast()
    {
        if (!bypassCastingManager && castingManager != null)
        {
            Fish detectedFish = castingManager.GetFishInArea();
            if (detectedFish != null)
            {
                return detectedFish;
            }
        }

        if (fallbackFish != null)
        {
            return fallbackFish;
        }

        Fish[] availableFish;
    #if UNITY_2023_1_OR_NEWER
        availableFish = FindObjectsByType<Fish>(FindObjectsSortMode.None);
    #else
        availableFish = FindObjectsOfType<Fish>();
    #endif
        if (availableFish.Length > 0)
        {
            return availableFish[UnityEngine.Random.Range(0, availableFish.Length)];
        }

        if (castingManager != null)
        {
            return castingManager.GetFishInArea();
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
    }

}