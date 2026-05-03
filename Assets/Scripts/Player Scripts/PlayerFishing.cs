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
    private Vector3 fishingPosition = new Vector3(-0.156808749f, 1.20062673f, -5.1311698f);
    private FishingRig fishingRig;
    private void Awake()
    {
        Instance = this;
        animator = GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            DebugLogger.Instance.LogWarning("PlayerFishing: No PlayerAnimator with an Animator component found in children.");
        }
        fishingRig = GetComponentInChildren<FishingRig>(true);
        if (fishingRig == null)
        {
            DebugLogger.Instance.LogWarning("PlayerFishing: No FishingRig found in children.");
        }
    }

    private void Start()
    {
        // PlayerInputState sets Player to Gameplay state on awake
        SetFishingActive(false);
    }

    private void OnEnable()
    {
        PlayerInputState.InteractPerformed += HandleInteract;
        FishingManager.OnHook += BeginReeling;
        FishingManager.OnCaught += HandleFishPresentation;
        FishingManager.OnReturnToGameplay += HandleFishingEnded;
    }

    private void OnDisable()
    {
        PlayerInputState.InteractPerformed -= HandleInteract;
        FishingManager.OnHook -= BeginReeling;
        FishingManager.OnCaught -= HandleFishPresentation;
        FishingManager.OnReturnToGameplay -= HandleFishingEnded;
    }

    private void HandleInteract()
    {
        if (IsFishing) return;

        // Calls AttemptCast which checks all conditions and returns false if fishing cannot be started (not on dock or not in idle state)
        if (CastingManager.Instance.AttemptCast())
        {
            // AttemptCast() -> fishingManager takes over (either failing or progressing to EnterCastingState()).
            // DebugLogger.Instance.Log("PlayerFishing: Fishing started successfully.");
            SetFishingActive(true);
            BeginCast(); // only handles animation and visuals for casting, while the FishingManager handles the actual game state and logic for casting.
        }
    }
    private void BeginCast()
    {
        // Debug.Log("PlayerFishing: Beginning cast animation and visuals.");
        transform.LookAt(Vector3.zero); // look forward towards water
        transform.position = fishingPosition;
        animator.SetTrigger("cast");
        animator.SetBool("isFishing", true);
        fishingRig.TriggerCast(); 
    }
    // Begin Reeling/Mashing Process
    private void BeginReeling()
    {
        // animator?.SetTrigger("reel");
        animator.SetBool("isReeling", true);
        animator.ResetTrigger("cast");
        fishingRig.TriggerReel(); 
    }
    private void HandleFishingEnded()
    {
        if (!IsFishing)
        {
            DebugLogger.Instance.LogWarning("HandleFishingEnded called but player is not in fishing state.");
            return;
        }

        // Keep player in fishing mode while presentations (catch and level up) are being shown
        FishingManager.FishingGameState currentState = FishingManager.Instance.CurrentFishingGameState;
        
        if (currentState == FishingManager.FishingGameState.CatchPresentation || currentState == FishingManager.FishingGameState.LevelUpPresentation)
        {
            DebugLogger.Instance.Log($"HandleFishingEnded called during {currentState}, ignoring to keep player in fishing mode for presentation.");
            return;
        }
        // Only exit fishing mode when returning to Gameplay after all presentations are confirmed
        DebugLogger.Instance.Log("HandleFishingEnded: Returning to Gameplay state and exiting fishing mode.");
        SetFishingActive(false);
        animator.SetBool("isReeling", false);
        animator.SetBool("isFishing", false);
        animator.SetBool("isPresenting", false);
    }
    private void HandleFishPresentation()
    {
        animator.SetBool("isReeling", false);
        animator.SetBool("isPresenting", true);
    }
    private void SetFishingActive(bool fishingActive)
    {
        IsFishing = fishingActive;

        // Set player input state to fishing mode
        PlayerInputState.Instance.SetState(fishingActive ? PlayerInputState.InputStates.Fishing : PlayerInputState.InputStates.Gameplay);
        // DebugLogger.Instance.Log($"PlayerFishing: Set fishing active: {fishingActive}. \nCurrent input state: {PlayerInputState.Instance.GetCurrentInputState()}");

        // enable/disable fishing visuals and player movement
        fishingRig.SetActive(fishingActive);
        PlayerMovement.Instance.enabled = !fishingActive;
    }
}