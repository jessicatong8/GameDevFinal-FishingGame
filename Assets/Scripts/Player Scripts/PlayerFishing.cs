using UnityEngine;

[RequireComponent(typeof(PlayerInputState))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFishing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject fishingRod;
    [SerializeField] private GameObject fishingLineObject;
    [SerializeField] private GameObject fishingHookObject;
    [SerializeField] private VerletLine fishingLine;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private PlayerInputState inputState;

    public bool IsFishing { get; private set; }
    private bool alreadyCast;

    private bool CanUseAnimator => animator != null && animator.runtimeAnimatorController != null && animator.isActiveAndEnabled;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (fishingLine == null)
        {
            fishingLine = GetComponentInChildren<VerletLine>(true);
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (fishingManager == null)
        {
            fishingManager = GetComponent<FishingManager>();
        }

        if (inputState == null)
        {
            inputState = GetComponent<PlayerInputState>();
        }

        if (!EnsureAnimatorReference())
        {
            Debug.LogWarning("PlayerFishing: No valid Animator with a controller was found on player visuals. Fishing animation parameters will be ignored until one is assigned.");
        }
    }

    private bool EnsureAnimatorReference()
    {
        if (CanUseAnimator)
        {
            return true;
        }

        Animator[] childAnimators = GetComponentsInChildren<Animator>(true);
        Animator fallbackAnimator = null;
        for (int i = 0; i < childAnimators.Length; i++)
        {
            Animator candidate = childAnimators[i];
            if (candidate == null || candidate.runtimeAnimatorController == null)
            {
                continue;
            }

            if (candidate.isActiveAndEnabled)
            {
                animator = candidate;
                return true;
            }

            if (fallbackAnimator == null)
            {
                fallbackAnimator = candidate;
            }
        }

        if (fallbackAnimator != null)
        {
            animator = fallbackAnimator;
            return true;
        }

        animator = null;
        return false;
    }

    private void Start()
    {
        if (inputState == null)
        {
            Debug.LogWarning("PlayerFishing requires a PlayerInputState component on the same object.");
            enabled = false;
            return;
        }

        inputState.SetState(PlayerInputState.InputStates.Gameplay);
        Debug.Log("PlayerFishing initialized. Starting in Gameplay state.");
        SetFishingState(false);
    }

    private void OnEnable()
    {
        if (inputState != null)
        {
            
            inputState.InteractPerformed += HandleInteract;
        }

        FishingManager.OnReelAttempt += BeginReel;
        FishingManager.OnReturnToIdle += HandleFishingEnded;
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.InteractPerformed -= HandleInteract;
        }

        FishingManager.OnReelAttempt -= BeginReel;
        FishingManager.OnReturnToIdle -= HandleFishingEnded;
    }

    private void HandleInteract()
    {
        if (!IsFishing)
        {
            if (fishingManager != null && !fishingManager.TryStartFishing())
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
        if (EnsureAnimatorReference())
        {
            animator.SetTrigger("cast");
        }

        fishingLine?.TriggerCast();
        alreadyCast = true;
    }

    private void BeginReel()
    {
        if (EnsureAnimatorReference())
        {
            animator.SetTrigger("reel");
        }

        fishingLine?.TriggerReel();
    }

    private void HandleFishingEnded()
    {
        if (!IsFishing)
        {
            return;
        }

        SetFishingState(false);
    }

    private void SetFishingState(bool active)
    {
        IsFishing = active;
        inputState.SetState(active ? PlayerInputState.InputStates.Fishing : PlayerInputState.InputStates.Gameplay);

        if (!active)
        {
            alreadyCast = false;
        }

        if (EnsureAnimatorReference())
        {
            animator.SetBool("startFishing", active);
        }

        if (fishingRod != null)
        {
            fishingRod.SetActive(active);
        }

        if (fishingLineObject != null)
        {
            fishingLineObject.SetActive(active);
        }

        if (fishingHookObject != null)
        {
            fishingHookObject.SetActive(active);
        }

        fishingLine?.SetEquippedFromController(active);

        if (playerMovement != null)
        {
            playerMovement.enabled = !active;
        }
    }
}