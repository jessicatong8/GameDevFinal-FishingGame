using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;

    public void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null)
        {
            DebugLogger.Instance.LogWarning("PlayerAnimator: No Animator component found on PlayerAnimator.");
        }
    }
}
