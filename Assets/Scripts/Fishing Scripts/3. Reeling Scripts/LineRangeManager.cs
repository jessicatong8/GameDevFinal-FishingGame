using UnityEngine;

public class LineRangeManager : MonoBehaviour
{
    public float xLineLeftWarningRange = -6f;
    public float xLineRightWarningRange = 6f;
    public float xLineLeftRange = -8f;
    public float xLineRightRange = 8f;
    private bool isInReelingState;
    public bool isInLineRange;

    private FishMovement activeFishMovement;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleResetToGameplay;
        FishingManager.OnReturnToGameplay += HandleResetToGameplay;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleResetToGameplay;
        FishingManager.OnReturnToGameplay -= HandleResetToGameplay;
    }


    private void HandleHooked()
    {
        if (FishingManager.Instance.activeFish == null)
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
        if (!isInReelingState) return;

        if (activeFishMovement == null)
        {
            DebugLogger.Instance.LogError("LineRangeManager: No active fish movement found during Update.");
            return;
        }

        if (activeFishMovement.IsOutOfLineRange())
        {
            DebugLogger.Instance.Log("LineRangeManager: Fish is out of line range!");
            FishingManager.Instance.EscapeFishing("OutOfLineRange");
            isInLineRange = false;
        }
        else
        {
            isInLineRange = true;
        }
    }

    private void HandleResetToGameplay()
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
