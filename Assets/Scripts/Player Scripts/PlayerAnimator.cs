using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PlayerAnimator>();
            }
            return instance;
        }
        private set => instance = value;
    } // Singleton instance for easy access from other scripts.
    public Animator animator;
    private static PlayerAnimator instance;

    public void Awake()
    {
        Instance = this;
        animator = GetComponentInChildren<Animator>();
        if (animator == null)        {
            DebugLogger.Instance.LogWarning("PlayerAnimator: No Animator found in children.");
        }
    }

}
