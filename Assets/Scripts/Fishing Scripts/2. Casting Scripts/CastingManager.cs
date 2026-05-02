using UnityEngine;

public class CastingManager : MonoBehaviour
{
    // Manages the game after the player casts
    // This encompasses the Casting and HookWindow game states
    // Selects the next fish from the FishSequenceManager
    // Coordinates the timing for when the fish bites and window during which the player must hook the fish
    // Does a drip check to determine if player succcessfully hooks the fish

    private static CastingManager instance;
    public static CastingManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<CastingManager>();
            return instance;
        }
        private set => instance = value;
    }

    Fish activeFish;

    private float biteTimer;
    public float minBiteDelay = 0.25f;
    public float maxBiteDelay = 2f;

    private float hookTimer;
    public float minHookWindow = 1.5f;
    public float maxHookWindow = 3f;

    [SerializeField] private bool disableDripCheck;


    private void Awake()
    {
        Instance = this;
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

    public bool TryStartFishing()
    {
        if (!IsPlayerInFishingArea() || FishingManager.Instance.CurrentFishingGameState != FishingManager.FishingGameState.Gameplay)
        {
            if (!IsPlayerInFishingArea())
            {
                DebugLogger.Instance.Log("Player is not in a fishing area and cannot start fishing.");
            }
            if (FishingManager.Instance.CurrentFishingGameState != FishingManager.FishingGameState.Gameplay)
            {
                DebugLogger.Instance.Log("Cannot start fishing because current state is " + FishingManager.Instance.CurrentFishingGameState + " instead of Gameplay.");
            }
            return false;
        }

        return EnterCastingState(); // returns true on success and false on failure (e.g. no fish in spot, drip too low)
    }


    private bool EnterCastingState()
    {
        activeFish = FishSequenceManager.Instance.GetNextFishFromSequence();
        if (activeFish == null)
        {
            DebugLogger.Instance.Log("No fish available to catch. Cannot start fishing.");
            return false;
        }

        activeFish.isActiveFish = true;
        FishingManager.Instance.activeFish = activeFish;

        // DebugLogger.Instance.LogMethodCall("FishingManager.EnterCastingState", "-> !OnCast\nCasting line with fish: " + activeFish.fishName);
        FishingManager.Instance.InvokeCast();

        SetBiteTimer();
        return true;
    }

    private void HandleHookPerformed()
    {
        DebugLogger.Instance.Log("HandleHookPerformed called. Current input state: " + PlayerInputState.Instance.CurrentState);
        if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.HookWindow)
        {
            if (IsPlayerDrippyEnough())
            {
                // DebugLogger.Instance.Log("HandleHookPerformed check passed.");
                DebugLogger.Instance.LogMethodCall("CastingManager.HandleHookPerformed", "-> !OnHook\nHook successful");
                FishingManager.Instance.InvokeHooked();
            }
            else
            {
                DebugLogger.Instance.LogMethodCall("CastingManager.HandleHookPerformed", "-> !OnHook\nHook failed. You are not drippy enough for this fish.");
                FishSequenceManager.Instance.IncrementFishSequenceIndex();
                FishingManager.Instance.EscapeFishing("DripLevelTooLow");

            }

        }
    }

    private bool IsPlayerDrippyEnough()
    {
        if (disableDripCheck)
        {
            return true;
        }
        return activeFish.level <= LevelManager.Instance.GetPlayerLevel();
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
