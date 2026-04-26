using UnityEngine;
using UnityEngine.UI;

public class TensionUI : MonoBehaviour
{
    
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private FishingManager fishingManager;

    [Header("UI Objects")]
    [SerializeField] private GameObject uiPanel; 
    [SerializeField] private Slider tensionSlider;
    [SerializeField] private RectTransform safeZoneOverlay; 

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

    private void SetupSafeZone()
    {
        Fish fish = fishingManager.activeFish;

        float halfWidth = fish.safeZoneWidth / 2f;
        float left = fish.safeZoneCenter - halfWidth;
        float right = fish.safeZoneCenter + halfWidth;

        safeZoneOverlay.anchorMin = new Vector2(left, 0);
        safeZoneOverlay.anchorMax = new Vector2(right, 1);
        
        safeZoneOverlay.offsetMin = Vector2.zero;
        safeZoneOverlay.offsetMax = Vector2.zero;
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