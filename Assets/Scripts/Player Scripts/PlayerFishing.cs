using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    // Handles player input and state transitions related to fishing, as well as triggering animations and visuals for casting and reeling.
    public static PlayerFishing Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PlayerFishing>();
            }
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts.
    public bool IsFishing;
    private static PlayerFishing instance;

    private Animator animator;
    private FishingRig fishingRig;
    private LineCastingVisuals lineCastingVisuals;
    [SerializeField] private float castReleaseDelay = 0.25f;
    private bool castReleased;
    private void Awake()
    {
        Instance = this;
        animator = GetComponentInChildren<Animator>();
        fishingRig = GetComponentInChildren<FishingRig>(true);
        if (fishingRig == null)
        {
            DebugLogger.Instance.LogWarning("PlayerFishing: No FishingRig found in children.");
        }
        lineCastingVisuals = GetComponentInChildren<LineCastingVisuals>(true);
        if (lineCastingVisuals == null)
        {
            DebugLogger.Instance.LogWarning("PlayerFishing: No LineCastingVisuals found in children.");
        }
    }

    private void Start()
    {
        PlayerInputState.Instance.SetState(PlayerInputState.InputStates.Gameplay);
        DebugLogger.Instance.Log("PlayerFishing: Starting in Gameplay state.");
        SetFishingActive(false);
    }

    private void OnEnable()
    {
        PlayerInputState.InteractPerformed += HandleInteract;

        FishingManager.OnHook += BeginReeling;
        FishingManager.OnReturnToIdle += HandleFishingEnded;
    }

    private void OnDisable()
    {
        PlayerInputState.InteractPerformed -= HandleInteract;

        FishingManager.OnHook -= BeginReeling;
        FishingManager.OnReturnToIdle -= HandleFishingEnded;
    }

    private void HandleInteract()
    {
        if (IsFishing) { return; }

        // Calls TryStartFishing which checks all conditions and returns false if fishing cannot be started (not on dock or not in idle state)
        if (FishingManager.Instance.TryStartFishing())
        {
            // DebugLogger.Instance.Log("PlayerFishing: Fishing started successfully.");
            // TryStartFishing() -> fishingManager takes over (either failing or progressing to EnterCastingState()).
            SetFishingActive(true);
            BeginCast();
        }
    }

    private void BeginCast()
    {
        castReleased = false;
        animator?.SetTrigger("cast");
        StartCoroutine(ReleaseCastFallback());
    }

    // TODO: Call this from an animation event at the frame where the cast should release.
    public void ReleaseCast()
    {
        if (castReleased)
        {
            return;
        }

        castReleased = true;
        DebugLogger.Instance.LogMethodCall("PlayerFishing.ReleaseCast");

        if (lineCastingVisuals == null)
        {
            DebugLogger.Instance.LogWarning("PlayerFishing.ReleaseCast: no LineCastingVisuals found, so the cast line cannot move.");
            return;
        }

        lineCastingVisuals.ReleaseCast();
    }

    private System.Collections.IEnumerator ReleaseCastFallback()
    {
        yield return new WaitForSeconds(castReleaseDelay);

        if (IsFishing && !castReleased)
        {
            ReleaseCast();
        }
    }

    // Begin Reeling/Mashing Process
    private void BeginReeling()
    {
        // animator?.SetTrigger("reel");
        animator?.SetBool("isReeling", true);
        lineCastingVisuals?.TriggerReel();
    }

    private void HandleFishingEnded()
    {
        castReleased = false;
        if (!IsFishing)
        {
            DebugLogger.Instance.LogWarning("HandleFishingEnded called but player is not in fishing state.");
            return;
        }
        SetFishingActive(false);
    }

    private void SetFishingActive(bool fishingActive)
    {
        IsFishing = fishingActive;

        // Set player input state to fishing mode
        PlayerInputState.Instance.SetState(fishingActive ? PlayerInputState.InputStates.Fishing : PlayerInputState.InputStates.Gameplay);
        DebugLogger.Instance.Log($"PlayerFishing: Set fishing active: {fishingActive}. \nCurrent input state: {PlayerInputState.Instance.GetCurrentInputState()}");

        // animator parameter for idle/fishing animation transition
        // animator?.SetBool("startFishing", fishingActive);
        if (fishingActive)
        {
            PlayerAnimator.Instance.animator?.SetTrigger("startFishing");
        }
        // enable/disable fishing visuals and player movement
        fishingRig?.SetActive(fishingActive);

        // if fishing -> movement disabled, if not fishing -> movement enabled
        PlayerMovement.Instance.enabled = !fishingActive;
    }
}