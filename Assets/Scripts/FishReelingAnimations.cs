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
        FishingManager.OnEscaped += HandleEscaped;

    }

    void OnDestroy()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleReelAttempt;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped;
    }

    void Update()
    {
    }

    void HandleBite()
    {
        animator.SetTrigger("Bite");
    }

    void HandleReelAttempt()
    {
        animator.SetBool("IsReeling", true);
    }

    void HandleCaught()
    {
        animator.SetTrigger("Caught");
    }

    void HandleLineBreak()
    {
        animator.SetTrigger("Escaped");
        animator.SetBool("IsReeling", false);
    }

    void HandleEscaped()
    {
        animator.SetTrigger("Escaped");
        animator.SetBool("IsReeling", false);
    }


}
