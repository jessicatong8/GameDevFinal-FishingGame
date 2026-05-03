using UnityEngine;

public class CastingManager : MonoBehaviour
{
    // Manages the game after the player casts
    // This encompasses the Casting and HookWindow game states
    // Selects the next fish from the FishSequenceManager
    // Coordinates the timing for when the fish bites and window during which the player must hook the fish
    // Does a drip check to determine if player succcessfully hooks the fish

    public float minBiteDelay = 0.25f;
    public float maxBiteDelay = 2f;
    public float minHookWindow = 1.5f;
    public float maxHookWindow = 3f;
    private float biteTimer;    // Timer for how long it takes for a fish to bite after casting (randomized between minBiteDelay and maxBiteDelay)
    private float hookTimer;    // Timer for how long the hook window lasts after the fish bites (randomized between minHookWindow and maxHookWindow)
    private Fish activeFish;
    [SerializeField] private bool disableDripCheck = false;     // for testing purposes, allows player to hook fish regardless of drip level
    private FishingManager fishingManager;
    private FishSequenceManager fishSequenceManager;
    private GroundChecker groundChecker;
    private LevelManager levelManager;
    private void Start()
    {
        fishingManager = FindFirstObjectByType<FishingManager>();
        fishSequenceManager = FindFirstObjectByType<FishSequenceManager>();
        groundChecker = FindFirstObjectByType<GroundChecker>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }
    private void OnEnable()
    {
        PlayerInputState.HookPerformed += HandleHookPerformed;
    }
    private void OnDisable()
    {
        PlayerInputState.HookPerformed -= HandleHookPerformed;
    }
    private void SetBiteTimer()
    {
        biteTimer = Random.Range(minBiteDelay, maxBiteDelay);
    }
    private void SetHookTimer()
    {
        hookTimer = Random.Range(minHookWindow, maxHookWindow);
    }
    private bool IsPlayerInFishingArea()
    {
        return FishingAreaTrigger.IsPlayerInFishingArea;
    }
    public bool AttemptCast()
    {
        if (!IsPlayerInFishingArea())
        {
            DebugLogger.Instance.Log("Player is not in a fishing area and cannot start fishing.");
            return false;
        }
        if (groundChecker != null && !groundChecker.IsGrounded)
        {
            DebugLogger.Instance.Log("Player is not grounded and cannot start fishing.");
            return false;
        }
        if (fishingManager.CurrentFishingGameState != FishingManager.FishingGameState.Gameplay)
        {
            DebugLogger.Instance.Log("CastingManager: AttemptCast failed. " + fishingManager.CurrentFishingGameState + " != Gameplay.");
            return false;
        }
        activeFish = fishSequenceManager.GetNextFishFromSequence();
        if (activeFish == null)
        {
            DebugLogger.Instance.Log("No fish available to catch. Cannot start fishing.");
            return false;
        }
        activeFish.isActiveFish = true;
        fishingManager.activeFish = activeFish;
        fishingManager.InvokeCast();

        SetBiteTimer();
        return true;
    }
    private void HandleHookPerformed()
    {
        // DebugLogger.Instance.Log("HandleHookPerformed called. Current input state: " + PlayerInputState.Instance.CurrentState);
        if (fishingManager.CurrentFishingGameState == FishingManager.FishingGameState.HookWindow)
        {
            if (IsPlayerDrippyEnough())
            {
                // DebugLogger.Instance.Log("HandleHookPerformed check passed.");
                // DebugLogger.Instance.LogMethodCall("CastingManager.HandleHookPerformed", "-> !OnHook\nHook successful");
                fishingManager.InvokeHooked();
            }
            else
            {
                DebugLogger.Instance.LogMethodCall("CastingManager.HandleHookPerformed", "-> !OnHook\nHook failed. You are not drippy enough for this fish.");
                fishSequenceManager.IncrementFishSequenceIndex();
                fishingManager.EscapeFishing("DripLevelTooLow");
            }
        }
    } 
    private bool IsPlayerDrippyEnough()
    {
        if (disableDripCheck)
        {
            return true;
        }
        return activeFish.level <= levelManager.GetPlayerLevel();
    }
    private void UpdateCastingTimer()
    {
        biteTimer -= Time.deltaTime;
        if (biteTimer <= 0)
        {
            FishingManager.Instance.InvokeBite();
            SetHookTimer();
        }
    }
    private void UpdateHookWindowTimer()
    {
        hookTimer -= Time.deltaTime;
        if (hookTimer <= 0)
        {
            FishingManager.Instance.EscapeFishing("HookWindowTimedOut");
        }
    }
    private void Update()

    {
        if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Casting)
        {
            UpdateCastingTimer();
        }
        else if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.HookWindow)
        {
            UpdateHookWindowTimer();
        }

    }
}
