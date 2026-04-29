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
        if (animator == null) animator = GetComponent<Animator>();
    }
}
