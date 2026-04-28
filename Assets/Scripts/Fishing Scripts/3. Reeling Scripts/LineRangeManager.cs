using UnityEngine;

public class LineRangeManager : MonoBehaviour
{
    public static LineRangeManager Instance { get; private set; }

    FishMovement activeFishMovement;

    public float xLineLeftWarningRange = -4f;
    public float xLineRightWarningRange = 4f;

    public float xLineLeftRange = -5f;
    public float xLineRightRange = 5f;

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
        activeFishMovement = FishingManager.Instance.activeFish.GetComponent<FishMovement>();
        if (activeFishMovement == null)
        {
            DebugLogger.Instance.LogError("LineRangeManager: No active fish found.");
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
