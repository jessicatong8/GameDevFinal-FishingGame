using UnityEngine;

public class FishAnimation : MonoBehaviour
{
    private Animator animator;
    // public FishingManager fishManager;

    void Start()
    {
        animator = GetComponent<Animator>();

        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleEscaped;
        FishingManager.OnReturnToGameplay += HandleResetToIdle;
    }

    void OnDestroy()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped; // can include optional escape animation instead
        FishingManager.OnReturnToGameplay -= HandleResetToIdle;
    }

    void Update()
    {
    }

    void HandleBite()
    {
        if (GetComponent<Fish>().isActiveFish)
        {
            animator.SetTrigger("Bite");
        }
    }

    void HandleHooked()
    {
        if (GetComponent<Fish>().isActiveFish)
        {
            animator.SetBool("IsHooked", true);
        }

    }

    void HandleCaught()
    {
        animator.SetBool("IsCaught", true);     // makes fish static for catch presentation
    }

    void HandleResetToIdle()
    {
        animator.SetBool("IsHooked", false);
        animator.SetBool("IsCaught", false);
    }

    void HandleEscaped() //trigger escape animation, then return to idle
    {
        animator.SetTrigger("Escaped");
        animator.SetBool("IsHooked", false);
    }
}
