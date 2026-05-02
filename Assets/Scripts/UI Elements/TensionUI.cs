using UnityEngine;
using UnityEngine.UI;

public class TensionUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private FishingManager fishingManager;

    [Header("UI")]
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
        FishingManager.OnReturnToGameplay += HandleHideUI;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleShowUI;
        FishingManager.OnCaught -= HandleHideUI;
        FishingManager.OnEscaped -= HandleHideUI;
        FishingManager.OnReturnToGameplay -= HandleHideUI;
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

    private void SetupSafeZone()
    {
        Fish fish = fishingManager.activeFish;

        float safeZoneWidth = fish.safeZoneWidth;
        float offsetFromCenter = fish.safeZoneCenter - 50f;
        float parentWidth = tensionSlider.GetComponent<RectTransform>().rect.width;

        safeZone.transform.localScale = new Vector3(safeZoneWidth/100, 1f, 1f);
        safeZone.localPosition = new Vector3((offsetFromCenter/100) * parentWidth, safeZone.localPosition.y, safeZone.localPosition.z);
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