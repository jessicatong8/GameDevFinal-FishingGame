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

    [Header("Rig Migration (Optional)")]
    [SerializeField] private GameObject fishingRodAssembly;
    [SerializeField] private string fishingLineChildPath = "Line";
    [SerializeField] private string fishingHookChildPath = "Hook";

    public bool IsFishing;
    private bool alreadyCast;

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
        SetFishingState(false);
    }

    private void OnEnable()
    {
        inputState.InteractPerformed += HandleInteract;

        FishingManager.OnHook += BeginReeling;
        FishingManager.OnReturnToIdle += HandleFishingEnded;
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.InteractPerformed -= HandleInteract;
        }

        FishingManager.OnHook -= BeginReeling;
        FishingManager.OnReturnToIdle -= HandleFishingEnded;
    }

    private void HandleInteract()
    {
        if (!inputState.GetCurrentInputState().Equals(PlayerInputState.InputStates.Fishing))
        {
            if (!fishingManager.TryStartFishing())
            {
                Debug.Log("Cannot start fishing: start conditions were not met.");
                return;
            }

            SetFishingState(true);
            BeginCast();
            return;
        }

        if (!alreadyCast)
        {
            Debug.Log("Player is fishing but has not cast yet. Triggering cast.");
            BeginCast();
            HandleFishingEnded();
        }
    }

    private void BeginCast()
    {
        animator?.SetTrigger("cast");
        fishingRig?.TriggerCast();
        alreadyCast = true;
    }

    // Begin Reeling/Mashing Process
    private void BeginReeling()
    {
        animator?.SetTrigger("reel");
        fishingRig?.TriggerReel();
    }

    private void HandleFishingEnded()
    {
        if (!IsFishing) { return; }
        SetFishingState(false);
    }

    private void SetFishingState(bool fishingActive)
    {
        IsFishing = fishingActive;

        // Set player input state to fishing mode
        inputState.SetState(fishingActive ? PlayerInputState.InputStates.Fishing : PlayerInputState.InputStates.Gameplay);

        // animator parameter for idle/fishing animation transition
        animator?.SetBool("startFishing", fishingActive);

        // enable/disable fishing visuals and player movement
        fishingRig?.SetActive(fishingActive);

        // enable/disable 
        playerMovement.enabled = !fishingActive;

        if (!fishingActive)
        {
            alreadyCast = false;
        }
    }
}