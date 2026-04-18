using UnityEngine;

[RequireComponent(typeof(PlayerInputState))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFishing : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    private PlayerInputState inputState;
    private Animator animator;
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private FishingRig fishingRig;

    public bool IsFishing;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        inputState = GetComponent<PlayerInputState>();

        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        inputState.SetState(PlayerInputState.InputStates.Gameplay);
        // Debug.Log("PlayerFishing: Starting in Gameplay state.");
        SetFishingActive(false);
    }

    private void OnEnable()
    {
        inputState.InteractPerformed += HandleInteract;

        FishingManager.OnHook += BeginReeling;
        FishingManager.OnReturnToIdle += HandleFishingEnded;
    }

    private void OnDisable()
    {

        inputState.InteractPerformed -= HandleInteract;

        FishingManager.OnHook -= BeginReeling;
        FishingManager.OnReturnToIdle -= HandleFishingEnded;
    }

    private void HandleInteract()
    {
        // Calls TryStartFishing which checks all conditions and returns false if fishing cannot be started (e.g. no fish in spot, drip too low, not on dock, not in idle state)
        if (!fishingManager.TryStartFishing())
        {
            Debug.Log("Cannot start fishing: start conditions were not met.");
            return;
        }
        else
        {
            // On TryStartFishing success, fishingManager takes over (either failing or progressing to EnterCastingState()).
            // Debug.Log("Fishing started successfully.");
            SetFishingActive(true);
            BeginCast();
        }
    }

    private void BeginCast()
    {
        animator?.SetTrigger("cast");
        // fishingRig?.TriggerCast();
    }

    // Begin Reeling/Mashing Process
    private void BeginReeling()
    {
        animator?.SetTrigger("reel");
        // fishingRig?.TriggerReel();
    }

    private void HandleFishingEnded()
    {
        if (!IsFishing)
        {
            Debug.LogWarning("HandleFishingEnded called but player is not in fishing state.");
            return;
        }
        SetFishingActive(false);
    }

    private void SetFishingActive(bool fishingActive)
    {
        IsFishing = fishingActive;

        // Set player input state to fishing mode
        inputState.SetState(fishingActive ? PlayerInputState.InputStates.Fishing : PlayerInputState.InputStates.Gameplay);

        // animator parameter for idle/fishing animation transition
        animator?.SetBool("startFishing", fishingActive);

        // enable/disable fishing visuals and player movement
        // fishingRig?.SetActive(fishingActive);

        // if fishing -> movement disabled, if not fishing -> movement enabled
        playerMovement.enabled = !fishingActive;
    }
}