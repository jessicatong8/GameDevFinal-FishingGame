using UnityEngine;

public class FishReelingAnimations : MonoBehaviour
{
    private Animator animator;
    public FishingManager fishManager;
    private Fish fish;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        fish = GetComponent<Fish>();

        // FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnReelingActive += HandleReelingActive;
        FishingManager.OnReelingInactive += HandleReelingInactive;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleLineBreak;
        FishingManager.OnWiggle += HandleWiggleStart;
        FishingManager.OffWiggle += HandleWiggleEnd;
        // FishingManager.OnReturnToIdle += HandleReturnToIdle;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void HandleBite()
    {
        if (fish.isActive)
        {
            Debug.Log(fish.name + " has bitten!");
            animator.SetBool("IsActive", true);
            animator.SetTrigger("Bite");
        }
    }

    void HandleWiggleStart()
    {
        animator.SetBool("IsWiggling", true);
    }

    void HandleWiggleEnd()
    {
        animator.SetBool("IsWiggling", false);
    }

    void HandleCaught()
    {
        animator.SetTrigger("Caught");
    }

    void HandleLineBreak()
    {
        animator.SetTrigger("Escaped");
    }

    void HandleReelingActive()
    {
        animator.SetBool("IsReeling", true);
    }

    void HandleReelingInactive()
    {
        animator.SetBool("IsReeling", false);
    }

    // void HandleReturnToIdle()
    // {
    //     animator.SetBool("IsActive", false);
    // }


}
