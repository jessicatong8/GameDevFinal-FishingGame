using UnityEngine;

public class LineRangeUI : MonoBehaviour
{


    [Header("UI Objects")]
    [SerializeField] private GameObject LeftArrow;
    [SerializeField] private GameObject RightArrow;

    [SerializeField] private GameObject WarningUI;
    [SerializeField] private GameObject SafeZone;

    Fish activeFish;

    private bool isInReelingState;

    private void Awake()
    {
        HandleDisableUI();
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleEnableUI;
        FishingManager.OnCaught += HandleDisableUI;
        FishingManager.OnEscaped += HandleDisableUI;
        FishingManager.OnReturnToIdle += HandleDisableUI;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleEnableUI;
        FishingManager.OnCaught -= HandleDisableUI;
        FishingManager.OnEscaped -= HandleDisableUI;
        FishingManager.OnReturnToIdle -= HandleDisableUI;
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
        // WarningUI.SetActive(false);
        SafeZone.SetActive(false);
    }

    private void HandleLeftWarning()
    {
        RightArrow.SetActive(true);
        //WarningUI.SetActive(true);

    }
    private void HandleRightWarning()
    {
        LeftArrow.SetActive(true);
        //WarningUI.SetActive(true);
    }
    private void HandleInInnerLineRange()
    {
        LeftArrow.SetActive(false);
        RightArrow.SetActive(false);
        //WarningUI.SetActive(false);
    }

    private void Update()
    {
        if (!isInReelingState)
        {
            return;
        }
        if (LineRangeManager.Instance.IsInLeftWarningRange())
        {
            // Debug.Log("In Left Warning Range");
            HandleLeftWarning();
        }
        else if (LineRangeManager.Instance.IsInRightWarningRange())
        {
            // Debug.Log("In Right Warning Range");
            HandleRightWarning();
        }
        else if (LineRangeManager.Instance.IsInInnerLineRange())
        {
            HandleInInnerLineRange();
        }

    }
}