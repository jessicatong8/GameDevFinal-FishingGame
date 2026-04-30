using UnityEditor;
using UnityEngine;

public class CastingManager : MonoBehaviour
{
    // Manages the game after the player casts to select the next fish from the FishSequenceManager and coordinate the timing for when the fish bites and window during which the player must hook the fish
    // This encompasses the Casting and HookWindow game states

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
    public float minBiteDelay = 1f;
    public float maxBiteDelay = 3f;

    private float hookTimer;
    public float minHookWindow = 1.54f;
    public float maxHookWindow = 3f;


    private void Awake()
    {
        Instance = this;
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
            FishingManager.Instance.EscapeFishing("Fish escaped because hook window timed out.");
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
