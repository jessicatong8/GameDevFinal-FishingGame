using UnityEngine;

public class FishReelingAnimations : MonoBehaviour
{
    private Animator animator;
    public FishingManager fishManager;

    void Start()
    {
        animator = GetComponent<Animator>();

        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleReelAttempt;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnCatchConfirmationEnd += HandleResetToIdle;
        FishingManager.OnEscaped += HandleResetToIdle;
        // FishingManager.OnReturnToIdle += HandleResetToIdle;
    }
    void OnDestroy()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleReelAttempt;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleResetToIdle; // can include optional escape animation instead
        FishingManager.OnCatchConfirmationEnd -= HandleResetToIdle;
        // FishingManager.OnReturnToIdle -= HandleResetToIdle;
    }
    void HandleBite()
    {
        animator.SetTrigger("Bite");
    }
    void HandleReelAttempt()
    {
        animator.SetBool("IsReeling", true);    // swimming fast animation 
    }
    void HandleCaught()
    {
        animator.SetBool("IsCaught", true);     // makes fish static for catch presentation
    }
    void HandleResetToIdle()
    {
        animator.SetBool("IsReeling", false);
        animator.SetBool("IsCaught", false);
    }
}
