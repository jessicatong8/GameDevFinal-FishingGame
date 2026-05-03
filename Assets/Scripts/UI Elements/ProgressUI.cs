using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressUI : MonoBehaviour
{
    [SerializeField] private ProgressManager progressManager;
    private Slider progressSlider;

    void OnEnable()
    {
        FishingManager.OnHook += HandleHook;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay;
    }

    void OnDisable()
    {

        FishingManager.OnHook -= HandleHook;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }

    void Start()
    {

        progressSlider = GetComponentInChildren<Slider>();
        progressSlider.gameObject.SetActive(false);
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
    void HandleHook()
    {

        progressSlider.gameObject.SetActive(true);
    }


    void HandleReturnToGameplay()
    {
        progressSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateProgressSlider(progressManager.GetCurrentProgress());
    }
}
