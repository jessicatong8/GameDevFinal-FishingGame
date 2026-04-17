using UnityEngine;

public class FishReelingAnimations : MonoBehaviour
{
    private Animator animator;
    public FishingManager fishManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleReelAttempt;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnLineBreak += HandleLineBreak;

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


}
