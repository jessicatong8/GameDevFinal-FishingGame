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

    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleReel;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleEscaped;
    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleReel;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleEscaped;

    }

    void Start()
    {
        UpdateProgressSlider(0);
        UpdateTensionSlider(0);
        UpdateTensionSliderMax(0);

    }
    void Update()
    {
        UpdateProgressSlider(progressManager.GetCurrentProgress());
        UpdateTensionSlider(tensionManager.GetCurrentTension());
        UpdateTensionSliderMax(tensionManager.GetCurrentMaxTension());
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
        UpdateTensionSliderMax(0);
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
        alertText.text = "FISH CAUGHT!";
        alertText.color = Color.green;
    }

    void HandleEscaped()
    {
        alertText.text = "FISH ESCAPED!";
        alertText.color = new Color(1f, 0.75f, 0.2f);
    }

}
