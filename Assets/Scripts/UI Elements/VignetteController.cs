using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Use this for URP

public class VignetteController : MonoBehaviour
{
    [Header("Volume Reference")]
    [SerializeField] private Volume globalVolume;
    private Vignette vignette;

    [Header("Settings")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 0.45f; 
    [SerializeField] private float transitionSpeed = 8f;


    private void Start()
    {
        if (globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = minIntensity;
        }
    }

    private void Update()
    {
        if (FishingManager.Instance.CurrentFishingGameState != FishingManager.FishingGameState.Reeling)
        {
            if (vignette.intensity.value != minIntensity) vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, minIntensity, Time.deltaTime * transitionSpeed);
            return;
        }

        float fishX = FishingManager.Instance.activeFish.transform.position.x;
        float xLimit = LineRangeManager.Instance.xLineLeftRange; 
        
        float distanceRatio = Math.Abs(fishX/xLimit);
        
        distanceRatio = Mathf.Clamp01(distanceRatio);
        //clamps to 1 if fish swims beyond red zone

        float targetIntensity = maxIntensity * distanceRatio;

        vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, targetIntensity, Time.deltaTime * transitionSpeed);
    }
}