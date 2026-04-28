using UnityEngine;
using UnityEngine.UI;

public class TensionUI : MonoBehaviour
{

    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private FishingManager fishingManager;

    [Header("UI Objects")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Slider tensionSlider;
    [SerializeField] private Transform safeZone;
    private float safeZoneLeft;
    private float safeZoneRight;

    private void Awake()
    {
        uiPanel.SetActive(false);
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleShowUI;
        FishingManager.OnCaught += HandleHideUI;
        FishingManager.OnEscaped += HandleHideUI;
        FishingManager.OnReturnToIdle += HandleHideUI;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleShowUI;
        FishingManager.OnCaught -= HandleHideUI;
        FishingManager.OnEscaped -= HandleHideUI;
        FishingManager.OnReturnToIdle -= HandleHideUI;
    }

    private void HandleShowUI()
    {
        SetupSafeZone();
        uiPanel.SetActive(true);
    }

    private void HandleHideUI()
    {
        uiPanel.SetActive(false);
    }

    //change to get from tension manager 
    private void SetupSafeZone()
    {
        Fish fish = fishingManager.activeFish;

        float safeZoneWidth = fish.safeZoneWidth;

        safeZone.transform.localScale = new Vector3(safeZoneWidth/100, 1f, 1f);
    }

    private void Update()
    {
        if (uiPanel.activeSelf == false)
        {
            return;
        }

        float currentTension = tensionManager.GetCurrentTension();
        tensionSlider.value = currentTension;
    }
}