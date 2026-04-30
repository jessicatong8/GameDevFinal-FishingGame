using UnityEngine;

public class CastingManager : MonoBehaviour
{
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

    [SerializeField] private Fish[] fishSequence;
    private int fishSequenceIndex;
    Fish activeFish;

    private float biteTimer;
    private float minBiteDelay = 2f;
    private float maxBiteDelay = 5f;

    private float hookTimer;
    private float minHookWindow = 1.54f;
    private float maxHookWindow = 3f;


    public enum FishingGameState
    {
        Gameplay, // default state when not fishing
        Casting,
        HookWindow,
        Reeling,
    }
    private FishingGameState currentFishingGameState = FishingGameState.Gameplay;




    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleCaught;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleCaught;
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
        if (!IsPlayerInFishingArea() || currentFishingGameState != FishingGameState.Gameplay)
        {
            if (!IsPlayerInFishingArea())
            {
                DebugLogger.Instance.Log("Player is not in a fishing area and cannot start fishing.");
            }
            // if (currentFishingGameState != FishingGameState.Gameplay)
            // {
            //     DebugLogger.Instance.Log("Cannot start fishing because current state is " + currentFishingGameState + " instead of Gameplay.");
            // }
            return false;
        }

        return EnterCastingState(); // returns true on success and false on failure (e.g. no fish in spot, drip too low)
    }


    private bool EnterCastingState()
    {
        activeFish = GetFishFromSequence();
        if (activeFish == null)
        {
            DebugLogger.Instance.Log("No fish available to catch. Cannot start fishing.");
            return false;
        }

        activeFish.isActiveFish = true;
        FishingManager.Instance.activeFish = activeFish;

        // DebugLogger.Instance.LogMethodCall("FishingManager.EnterCastingState", "-> !OnCast\nCasting line with fish: " + activeFish.fishName);
        FishingManager.Instance.InvokeCast();

        // SetFishingGameState(FishingGameState.Casting);
        SetBiteTimer();
        currentFishingGameState = FishingGameState.Casting;
        return true;
    }


    private Fish GetFishFromSequence()
    {
        if (fishSequence == null || fishSequence.Length == 0)
        {
            DebugLogger.Instance.LogWarning("FishingManager: Fish sequence is null or empty, but usePrototypeFishSequence is true. Please assign fish to the sequence in the inspector.");
            return null;
        }


        if (fishSequenceIndex < fishSequence.Length)
        {
            Fish nextFish = fishSequence[fishSequenceIndex];
            if (nextFish != null)
            {
                return nextFish;
            }

        }
        return null;
    }



    private void TickCastingState()
    {
        biteTimer -= Time.deltaTime;
        if (biteTimer <= 0)
        {
            FishingManager.Instance.InvokeBite();
            currentFishingGameState = FishingGameState.HookWindow;
            SetHookTimer();
        }
    }

    private void TickHookWindowState()
    {
        hookTimer -= Time.deltaTime;
        if (hookTimer <= 0)
        {
            FishingManager.Instance.EscapeFishing("Fish escaped because hook window timed out.");
            currentFishingGameState = FishingGameState.Gameplay;
        }
    }

    private void HandleHooked()
    {
        currentFishingGameState = FishingGameState.Reeling;
    }

    private void HandleCaught()
    {
        fishSequenceIndex++;
        currentFishingGameState = FishingGameState.Gameplay;
    }

    private void Update()
    {
        switch (currentFishingGameState)
        {
            case FishingGameState.Gameplay:
                break;

            case FishingGameState.Casting:
                TickCastingState();
                break;

            case FishingGameState.HookWindow:
                TickHookWindowState();
                break;

            case FishingGameState.Reeling:
                break;
        }
    }
}
