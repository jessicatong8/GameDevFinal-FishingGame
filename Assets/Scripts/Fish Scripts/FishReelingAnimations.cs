using UnityEngine;

public class FishReelingAnimations : MonoBehaviour
{
    private Animator animator;
    public FishingManager fishManager;
    private Fish fish;

    void Start()
    {
        animator = GetComponent<Animator>();
        fish = GetComponent<Fish>();

        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHook;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleEscaped;
        FishingManager.OnReturnToIdle += HandleEscaped;

    }

    void OnDestroy()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHook;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped;
        FishingManager.OnReturnToIdle -= HandleEscaped;
    }

    void Update()
    {
    }

    void HandleBite()
    {
        if (fish.isActiveFish)
        {
            animator.SetTrigger("Bite");
        }
    }

    void HandleHook()
    {
        if (fish.isActiveFish)
        {
            animator.SetBool("IsHooked", true);
        }

    }

    void HandleCaught()
    {

        animator.SetTrigger("Caught");

    }

    void HandleEscaped()
    {

        animator.SetTrigger("Escaped");
        animator.SetBool("IsHooked", false);

    }

}
