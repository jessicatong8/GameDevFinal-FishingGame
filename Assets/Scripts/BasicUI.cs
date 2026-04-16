using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class BasicUI : MonoBehaviour
{
    public FishingManager fishManager;
    
    public Slider progressSlider;
    public Slider tensionSlider;
    public TextMeshProUGUI alertText; 

    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnReel += HandleReel;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnLineBreak += HandleLineBreak;
        FishingManager.OnWiggle += HandleWiggle;
        FishingManager.OffWiggle += HandleWiggleEnd;

        FishingManager.OnProgressUpdated += UpdateProgressSlider;
        FishingManager.OnTensionUpdated += UpdateTensionSlider;
        FishingManager.OnTensionMaxUpdated += UpdateTensionSliderMax;
    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnReel -= HandleReel;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnLineBreak -= HandleLineBreak;
        FishingManager.OnWiggle -= HandleWiggle;
        FishingManager.OffWiggle -= HandleWiggleEnd;
        FishingManager.OnProgressUpdated -= UpdateProgressSlider;
        FishingManager.OnTensionUpdated -= UpdateTensionSlider;
        FishingManager.OnTensionMaxUpdated -= UpdateTensionSliderMax;
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
        alertText.text = "BITE! Press Space!";
    }

    void HandleReel()
    {
        alertText.text = "Start Reeling!";
    }

    void HandleCaught()
    {
        alertText.text = "FISH CAUGHT!";
        alertText.color = Color.green;
    }

    void HandleLineBreak()
    {
        alertText.text = "LINE SNAPPED!";
        alertText.color = Color.gray;
    }

    void HandleWiggle()
    {
        alertText.color = Color.red;
    }

    void HandleWiggleEnd()
    {
        alertText.color = Color.white;
    }
}
