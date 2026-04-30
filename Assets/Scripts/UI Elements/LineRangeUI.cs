using UnityEngine;

public class LineRangeUI : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject LeftArrow;
    [SerializeField] private GameObject RightArrow;
    [SerializeField] private GameObject WarningUI;
    [SerializeField] private GameObject SafeZone;
    private LineRangeManager lineRangeManager;
    private Fish activeFish;
    private bool isInReelingState;
    private void Start()
    {
        lineRangeManager = LineRangeManager.Instance;
        if (lineRangeManager == null)
        {
            DebugLogger.Instance.LogError("LineRangeUI: LineRangeManager instance not found in scene.");
        }
        HandleDisableUI();
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleEnableUI;
        FishingManager.OnCaught += HandleDisableUI;
        FishingManager.OnReturnToGameplay += HandleDisableUI;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleEnableUI;
        FishingManager.OnCaught -= HandleDisableUI;
        FishingManager.OnReturnToGameplay -= HandleDisableUI;
    }

    private void HandleEnableUI()
    {
        isInReelingState = true;
        activeFish = FishingManager.Instance.activeFish;
        if (activeFish == null)
        {
            DebugLogger.Instance.LogError("TensionManager: No active fish found.");
            return;
        }
        
        LeftArrow.SetActive(true);
        RightArrow.SetActive(true);
        WarningUI.SetActive(true);
        SafeZone.SetActive(true);
    }
    private void HandleDisableUI()
    {
        isInReelingState = false;
        activeFish = null;

        LeftArrow.SetActive(false);
        RightArrow.SetActive(false);
        WarningUI.SetActive(false);
        SafeZone.SetActive(false);
    }
    // private void HandleLeftWarning()
    // {
    //     RightArrow.SetActive(true);
    //     WarningUI.SetActive(true);

    // }
    // private void HandleRightWarning()
    // {
    //     LeftArrow.SetActive(true);
    //     WarningUI.SetActive(true);
    // }
    // private void HandleInInnerLineRange()
    // {
    //     LeftArrow.SetActive(false);
    //     RightArrow.SetActive(false);
    //     WarningUI.SetActive(false);
    // }
    private void Update()
    {
        if (!isInReelingState || lineRangeManager == null) return;

        bool isLeft = lineRangeManager.IsInLeftWarningRange();
        bool isRight = lineRangeManager.IsInRightWarningRange();
        bool isInner = lineRangeManager.IsInInnerLineRange();

        // Only update UI if something actually changed
        if (WarningUI.activeSelf != (!isInner))
        {
            WarningUI.SetActive(!isInner);
        }
        // Toggle arrows based on side
        if (LeftArrow.activeSelf != isRight) LeftArrow.SetActive(isRight);
        if (RightArrow.activeSelf != isLeft) RightArrow.SetActive(isLeft);
    }
}