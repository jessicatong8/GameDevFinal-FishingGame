using UnityEngine;

public class LineRangeUI : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject LineRangeIndicators;
    private GameObject LeftArrow;
    private GameObject RightArrow;
    [SerializeField] private GameObject LineRangeWaterOverlay;
    private GameObject WarningZone;
    private LineRangeManager lineRangeManager;
    private Fish activeFish;
    private bool isInReelingState;
    private void OnEnable()
    {
        FishingManager.OnHook += HandleEnableUI;
        FishingManager.OnCaught += HandleDisableUI;
        FishingManager.OnReturnToGameplay += HandleDisableUI;
    }
    private void Start()
    {
        lineRangeManager = LineRangeManager.Instance;
        if (lineRangeManager == null)
        {
            DebugLogger.Instance.LogError("LineRangeUI: LineRangeManager instance not found in scene.");
        }
        WarningZone = LineRangeWaterOverlay.transform.Find("WarningZone")?.gameObject;
        if (WarningZone == null)
        {
            DebugLogger.Instance.LogError("LineRangeUI: WarningZone child not found under LineRangeWaterOverlay.");
        }
        LeftArrow = LineRangeIndicators.transform.Find("LeftArrow")?.gameObject;
        if (LeftArrow == null)
        {
            DebugLogger.Instance.LogError("LineRangeUI: LeftArrow child not found under LineRangeIndicators.");
        }
        RightArrow = LineRangeIndicators.transform.Find("RightArrow")?.gameObject;
        if (RightArrow == null)
        {
            DebugLogger.Instance.LogError("LineRangeUI: RightArrow child not found under LineRangeIndicators.");
        }
        HandleDisableUI();
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
        LineRangeIndicators.SetActive(true);
        LineRangeWaterOverlay.SetActive(true);
    }
    private void HandleDisableUI()
    {
        isInReelingState = false;
        activeFish = null;

        LineRangeIndicators.SetActive(false);
        LineRangeWaterOverlay.SetActive(false);

    }
    private void Update()
    {
        if (!isInReelingState || lineRangeManager == null) return;

        bool isLeft = lineRangeManager.IsInLeftWarningRange();
        bool isRight = lineRangeManager.IsInRightWarningRange();
        bool isInner = lineRangeManager.IsInInnerLineRange();
        bool isOuter = !isInner; 

        // Only update UI if something actually changed
        if (WarningZone.activeSelf == true && isInner == true)
        {
            WarningZone.SetActive(false);
        }
        else if (WarningZone.activeSelf == false && isOuter == true)
        {
            WarningZone.SetActive(true);
        }
        // Toggle arrows based on side
        if (LeftArrow.activeSelf != isRight) LeftArrow.SetActive(isRight);
        if (RightArrow.activeSelf != isLeft) RightArrow.SetActive(isLeft);
    }
}