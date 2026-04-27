using UnityEngine;

[RequireComponent(typeof(PlayerInputState))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFishing : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    private Animator animator;
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private FishingRig fishingRig;

    public bool IsFishing;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
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
        if (fishingManager.TryStartFishing())
        {
            // DebugLogger.Instance.Log("PlayerFishing: Fishing started successfully.");
            // TryStartFishing() -> fishingManager takes over (either failing or progressing to EnterCastingState()).
            SetFishingActive(true);
            BeginCast();
        }
    }

    private void BeginCast()
    {
        animator?.SetTrigger("cast");
    }

    // TODO: Call this from an animation event at the frame where the cast should release.
    public void ReleaseCast()
    {
        DebugLogger.Instance.LogMethodCall("PlayerFishing.ReleaseCast");
        fishingRig?.TriggerCast();
    }

    // Begin Reeling/Mashing Process
    private void BeginReeling()
    {
        animator?.SetTrigger("reel");
        fishingRig?.TriggerReel();
    }

    private void HandleFishingEnded()
    {
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
        animator?.SetBool("startFishing", fishingActive);

        // enable/disable fishing visuals and player movement
        fishingRig?.SetActive(fishingActive);

        // if fishing -> movement disabled, if not fishing -> movement enabled
        playerMovement.enabled = !fishingActive;
    }
}