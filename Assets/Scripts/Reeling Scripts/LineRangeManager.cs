using UnityEngine;
using UnityEngine.InputSystem;

public class LineRangeManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private FishingManager fishingManager;
    Fish activeFish;

    public float xLineLeftWarningRange = -4f;
    public float xLineRightWarningRange = 4f;

    public float xLineLeftRange = -5f;
    public float xLineRightRange = 5f;



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
        activeFish = fishingManager.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("LineRangeManager: No active fish found.");
            return;
        }
    }

    private void HandleResetToIdle()
    {
        activeFish = null;
    }

    public bool IsInLineRange()
    {
        return activeFish.GetComponent<FishMovement>().IsInLineRange();
    }

    public bool IsInLeftWarningRange()
    {
        return activeFish.GetComponent<FishMovement>().IsInLeftWarningRange();
    }
    public bool IsInRightWarningRange()
    {
        return activeFish.GetComponent<FishMovement>().IsInRightWarningRange();
    }
}
