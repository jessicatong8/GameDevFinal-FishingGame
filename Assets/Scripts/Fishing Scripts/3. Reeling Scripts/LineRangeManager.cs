using UnityEngine;

public class LineRangeManager : MonoBehaviour
{
    public static LineRangeManager Instance { get; private set; }

    FishMovement activeFishMovement;

    public float xLineLeftWarningRange = -6f;
    public float xLineRightWarningRange = 6f;

    public float xLineLeftRange = -8f;
    public float xLineRightRange = 8f;

    private bool isInReelingState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate LineRangeManager found. Destroying extra instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnReturnToIdle += HandleResetToIdle;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnReturnToIdle -= HandleResetToIdle;

    }


    private void HandleHooked()
    {
        if (FishingManager.Instance == null || FishingManager.Instance.activeFish == null)
        {
            DebugLogger.Instance.LogError("LineRangeManager: No active fish available when hooked.");
            return;
        }

        activeFishMovement = FishingManager.Instance.activeFish.GetComponent<FishMovement>();
        if (activeFishMovement == null)
        {
            DebugLogger.Instance.LogError("LineRangeManager: Active fish has no FishMovement component.");
            return;
        }
        isInReelingState = true;
    }

    void Update()
    {
        if (!isInReelingState || activeFishMovement == null)
            return;

        if (activeFishMovement.IsOutOfLineRange())
        {
            DebugLogger.Instance.Log("Fish is out of line range!");
            FishingManager.Instance.EscapeFishing("Fish escaped because if went out of the line range");
        }
    }

    private void HandleResetToIdle()
    {
        activeFishMovement = null;
        isInReelingState = false;
    }

    public bool IsInInnerLineRange()
    {
        return activeFishMovement.IsInInnerLineRange();
    }

    public bool IsOutOfLineRange()
    {
        return activeFishMovement.IsOutOfLineRange();
    }

    public bool IsInLeftWarningRange()
    {
        return activeFishMovement.IsInLeftWarningRange();
    }
    public bool IsInRightWarningRange()
    {
        return activeFishMovement.IsInRightWarningRange();
    }
}
