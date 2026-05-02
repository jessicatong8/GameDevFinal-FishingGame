using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasicUI : MonoBehaviour
{
    private FishingManager fishingManager;
    [SerializeField] private ProgressManager progressManager;
    private Slider progressSlider;
    private TextMeshProUGUI statusText;

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
        PlayerInputState.MenuTogglePerformed -= HandleReturnToGameplay;
    }

    void Start()
    {
        // overlayCanvasGroup = GetComponent<CanvasGroup>();
        fishingManager = FishingManager.Instance;

        statusText = GetComponentInChildren<TextMeshProUGUI>(true);
        progressSlider = GetComponentInChildren<Slider>(true);
        HandleFishingGameStateChanged(fishingManager.CurrentFishingGameState);
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
        // Color txtcColor= new Color(0.447f, 0.851f, 0.447f, 1.000f);
        Color txtColor = new Color(1.000f, 1.000f, 1.000f, 1.000f);
        string txtColorHEX = ColorUtility.ToHtmlStringRGBA(txtColor);
        statusText.text = $"FISH CAUGHT: <color=#{txtColorHEX}>{fishingManager.activeFish.fishName}</color>";
    }
    void HandleEscaped()
    {
        statusText.text = "FISH ESCAPED!";
        statusText.color = new Color(1f, 0.75f, 0.2f);
    }
    void HandleFishingGameStateChanged(FishingManager.FishingGameState state)
    {
        bool isInMenu = PlayerInputState.Instance.CurrentState == PlayerInputState.InputStates.Menu;
        bool isFishing = state != FishingManager.FishingGameState.Gameplay;
        if (isFishing)
        {
            // FishingGameState.Bite, FishingGameState.Reeling, or FishingGameState.CatchPresentation
            progressSlider.gameObject.SetActive(state == FishingManager.FishingGameState.Reeling);
        }
        else
        {
            // FishingGameState.Gameplay (initial state)
            statusText.gameObject.SetActive(true);
            progressSlider.gameObject.SetActive(false);
        }
    }
    void HandleReturnToGameplay()
    {
        HandleFishingGameStateChanged(FishingManager.FishingGameState.Gameplay);
        UpdateProgressSlider(0);
        statusText.text = "Press F to Fish";
    }
    void HandleMenu()
    {
        HandleFishingGameStateChanged(FishingManager.FishingGameState.Gameplay);
        UpdateProgressSlider(0);
    }
    void Update()
    {
        UpdateProgressSlider(progressManager.GetCurrentProgress());
    }
}
