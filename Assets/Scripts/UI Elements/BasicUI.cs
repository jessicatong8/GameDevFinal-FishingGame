using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class BasicUI : MonoBehaviour
{
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private ProgressManager progressManager;


    private Slider progressSlider;
    private TextMeshProUGUI statusText;

    private CanvasGroup overlayCanvasGroup;

    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleReel;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleEscaped;
        FishingManager.OnFishingGameStateChanged += HandleFishingGameStateChanged;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay;
    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleReel;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped;
        FishingManager.OnFishingGameStateChanged -= HandleFishingGameStateChanged;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }

    void Start()
    {
        overlayCanvasGroup = GetComponent<CanvasGroup>();
        fishingManager = FishingManager.Instance;

        statusText = GetComponentInChildren<TextMeshProUGUI>(true);
        progressSlider = GetComponentInChildren<Slider>(true);
        UpdateProgressSlider(0);

        if (fishingManager != null)
        {
            HandleFishingGameStateChanged(fishingManager.CurrentFishingGameState);
        }
        else
        {
            progressSlider.gameObject.SetActive(false);
        }
    }
    void UpdateProgressSlider(float val)
    {
        if (progressSlider == null)
        {
            DebugLogger.Instance.LogError("BasicUI: No progress slider found!");
            return;
        }
        progressSlider.value = val;
    }
    void HandleCast()
    {
        statusText.gameObject.SetActive(true);
        statusText.text = "Casting";
        statusText.color = Color.white;
        UpdateProgressSlider(0);
    }
    void HandleBite()
    {
        statusText.text = "BITE! Press Reel!";
        progressSlider.gameObject.SetActive(true);
    }
    void HandleReel()
    {
        statusText.text = "MASH TO REEL!";
    }
    void HandleCaught()
    {
        // NOW HANDLED BY CATCH PRESENTATION UI

        statusText.text = "";
        // statusText.text = "FISH CAUGHT!";
        // statusText.color = Color.green;
    }
    void HandleEscaped()
    {
        statusText.text = "FISH ESCAPED!";
        statusText.color = new Color(1f, 0.75f, 0.2f);
    }
    void HandleFishingGameStateChanged(FishingManager.FishingGameState state)
    {
        bool isFishing = state != FishingManager.FishingGameState.Gameplay;
        if (isFishing)
        {
            // FishingGameState.Bite, FishingGameState.Reeling, or FishingGameState.CatchPresentation
            progressSlider.gameObject.SetActive(state == FishingManager.FishingGameState.Reeling);
        }
        else
        {
            // FishingGameState.Gameplay
            statusText.gameObject.SetActive(true);
            progressSlider.gameObject.SetActive(false);
        }
    }
    void HandleReturnToGameplay()
    {
        HandleFishingGameStateChanged(FishingManager.FishingGameState.Gameplay);
        UpdateProgressSlider(0);
    }
    void Update()
    {
        UpdateProgressSlider(progressManager.GetCurrentProgress());
    }
}
