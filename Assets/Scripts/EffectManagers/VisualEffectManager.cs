using UnityEngine;
using System.Collections;

public class VisualEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    [SerializeField] private GameObject biteSplash;
    [SerializeField] private GameObject mashSplash1;
    [SerializeField] private GameObject mashSplash2;

    //change based on andys line logic
    [Header("Spawn Location")]
    public Transform bobPoint;
    public Transform mashPoint;

    [Header("Screen Shake Reference")]
    [SerializeField] private ScreenShake screenShake;
    [Header("Sparkle Effect Prefab")]
    [SerializeField] private GameObject sparklePrefab;
    [SerializeField] private GameObject shinePrefab;
    [Header("Catch Presentation VFX Offsets")]
    [SerializeField] private Vector3 VFXposition = new Vector3(-0.150000006f, 3.57999992f, -3.73000002f);     
    [SerializeField] private Vector3 VFXscale = new Vector3(0.9f, 0.9f, 0.9f); 
    private GameObject sparkleInstance;
    private GameObject shineInstance;

    private void OnEnable()
    {
        FishingManager.OnBite += PlayBiteSplash;
        FishingManager.OnCaught += SpawnCatchPresentationVFX;
        FishingManager.OnEscaped += PlayEscapeShake;
        PlayerInputState.MashPerformed += PlayMashSplash;
        PlayerInputState.ConfirmCatchPerformed += DestroyCatchPresentationVFX;
    }

    private void OnDisable()
    {
        FishingManager.OnBite -= PlayBiteSplash;
        FishingManager.OnCaught -= SpawnCatchPresentationVFX;
        FishingManager.OnEscaped -= PlayEscapeShake;
        PlayerInputState.MashPerformed -= PlayMashSplash;
        PlayerInputState.ConfirmCatchPerformed -= DestroyCatchPresentationVFX;
    }
    void PlayBiteSplash()
    {
        SpawnSplash(biteSplash, 1, bobPoint.position);
    }

    void PlayMashSplash()
    {
        SpawnSplash(Random.Range(0, 2) == 0 ? mashSplash1 : mashSplash2, 1f, mashPoint.position);
    }

    void PlayEscapeShake()
    {
        screenShake.TriggerLargeShake(0.8f, 1f);
    }
    void SpawnSplash(GameObject prefab, float scale, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = Vector3.one * scale;
    }
    void SpawnCatchPresentationVFX()
    {
        sparkleInstance = Instantiate(sparklePrefab, VFXposition, Quaternion.identity);
        shineInstance = Instantiate(shinePrefab, VFXposition, Quaternion.identity);
        sparkleInstance.transform.localScale = VFXscale;
        shineInstance.transform.localScale = VFXscale;
    }
    void DestroyCatchPresentationVFX()
    {
        if (sparkleInstance != null)
        {
            Destroy(sparkleInstance);
        }
        else
        {
            Debug.LogWarning("No sparkle instance found to destroy.");
        }
        if (shineInstance != null)
        {
            Destroy(shineInstance);
        }
        else
        {
            Debug.LogWarning("No shine instance found to destroy.");
        }
    }
}