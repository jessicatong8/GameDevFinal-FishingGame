using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasicUI : MonoBehaviour
{
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] public TensionManager tensionManager;
    [SerializeField] public ProgressManager progressManager;


    public Slider progressSlider;
    public Slider tensionSlider;
    public TextMeshProUGUI alertText;

    private CanvasGroup overlayCanvasGroup;

    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleReel;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleEscaped;
        FishingManager.OnFishingGameStateChanged += HandleFishingGameStateChanged;
        FishingManager.OnReturnToIdle += HandleReturnToIdle;
    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleReel;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped;
        FishingManager.OnFishingGameStateChanged -= HandleFishingGameStateChanged;
        FishingManager.OnReturnToIdle -= HandleReturnToIdle;

    }

    void Start()
    {
        overlayCanvasGroup = GetComponent<CanvasGroup>();
        if (overlayCanvasGroup == null)
        {
            overlayCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        UpdateProgressSlider(0);
        UpdateTensionSlider(0);
        UpdateTensionSliderMax(100);

        if (fishingManager != null)
        {
            HandleFishingGameStateChanged(fishingManager.CurrentFishingGameState);
        }
        else
        {
            SetOverlayVisible(false);
        }

    }
    void Update()
    {
        UpdateProgressSlider(progressManager.GetCurrentProgress());
        UpdateTensionSlider(tensionManager.GetCurrentTension());
    }

    void UpdateProgressSlider(float val)
    {
        progressSlider.value = val;
    }

    void UpdateTensionSlider(float val)
    {
        tensionSlider.value = val;
    }

    void UpdateTensionSliderMax(float val)
    {
        tensionSlider.maxValue = Mathf.Max(val, 0f);
    }

    void HandleCast()
    {
        alertText.text = "Casting";
        alertText.color = Color.white;
        UpdateProgressSlider(0);
        UpdateTensionSlider(0);
        UpdateTensionSliderMax(100);
    }

    void HandleBite()
    {
        alertText.text = "BITE! Press Reel!";
    }

    void HandleReel()
    {
        alertText.text = "MASH TO REEL!";
    }

    void HandleCaught()
    {
        // NOW HANDLED BY CATCH PRESENTATION UI
        
        alertText.text = "";
        // alertText.text = "FISH CAUGHT!";
        // alertText.color = Color.green;
    }

    void HandleEscaped()
    {
        alertText.text = "FISH ESCAPED!";
        alertText.color = new Color(1f, 0.75f, 0.2f);
    }

    void HandleReturnToIdle()
    {
        HandleFishingGameStateChanged(FishingManager.FishingGameState.Idle);
        UpdateProgressSlider(0);
        UpdateTensionSlider(0);
        UpdateTensionSliderMax(100);
    }

    void HandleFishingGameStateChanged(FishingManager.FishingGameState state)
    {
        bool isFishing = state != FishingManager.FishingGameState.Idle;
        SetOverlayVisible(isFishing);
    }

    void SetOverlayVisible(bool visible)
    {
        if (overlayCanvasGroup == null)
        {
            return;
        }

        overlayCanvasGroup.alpha = visible ? 1f : 0f;
        overlayCanvasGroup.interactable = visible;
        overlayCanvasGroup.blocksRaycasts = visible;
    }

}
