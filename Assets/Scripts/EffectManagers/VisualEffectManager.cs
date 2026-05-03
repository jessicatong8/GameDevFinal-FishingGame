using UnityEngine;
using System.Collections;

public class VisualEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    // [SerializeField] private GameObject castSplash;           //less necessary 
    [SerializeField] private GameObject biteSplash;
    [SerializeField] private GameObject mashSplash1;
    [SerializeField] private GameObject mashSplash2;

    //change based on andys line logic
    [Header("Spawn Location")]
    public Transform bobPoint;
    public Transform mashPoint;

    [Header("Screen Shake Reference")]
    [SerializeField] private ScreenShake screenShake;

    [Header("VFX Offsets")]
    [SerializeField] private Vector3 VFXposition = new Vector3(-0.150000006f, 3.57999992f, -3.73000002f);
    [SerializeField] private Vector3 VFXscale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Catch Presentation VFX Prefabs")]
    [SerializeField] private GameObject catchSparklePrefab;
    [SerializeField] private GameObject catchShinePrefab;

    [Header("Level Up VFX Prefabs")]
    [SerializeField] private GameObject levelUpSparklePrefab;
    [SerializeField] private GameObject levelUpShinePrefab;
    private GameObject catchSparkleInstance;
    private GameObject catchShineInstance;
    private GameObject levelUpSparkleInstance;
    private GameObject levelUpShineInstance;

    private void OnEnable()
    {
        FishingManager.OnCast += PlayCastSplash;
        FishingManager.OnBite += PlayBiteSplash;
        FishingManager.OnCaught += SpawnCatchPresentationVFX;
        FishingManager.OnEscaped += PlayEscapeShake;
        PlayerInputState.MashPerformed += PlayMashSplash;
        PlayerInputState.ConfirmPerformed += DestroyCatchPresentationVFX;
        LevelManager.OnLevelUp += SpawnLevelUpPresentationVFX; // also play mash splash on level up for that extra satisfying feedback
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSplash;
        FishingManager.OnBite -= PlayBiteSplash;
        FishingManager.OnCaught -= SpawnCatchPresentationVFX;
        FishingManager.OnEscaped -= PlayEscapeShake;
        PlayerInputState.MashPerformed -= PlayMashSplash;
        PlayerInputState.ConfirmPerformed -= DestroyCatchPresentationVFX;
        LevelManager.OnLevelUp -= SpawnLevelUpPresentationVFX;
    }
    void PlayCastSplash()
    {
        // StartCoroutine(DelayedSpawn(castSplash, 3.5f, 0.3f, bobPoint.position));
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
        screenShake.TriggerLargeShake(0.6f, 1f);
    }
    // private IEnumerator DelayedSpawn(GameObject prefab, float delay, float scale, Vector3 position)
    // {
    //     yield return new WaitForSeconds(delay);
    //     SpawnSplash(prefab, scale, position);
    // }
    void SpawnSplash(GameObject prefab, float scale, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = Vector3.one * scale;
    }
    void SpawnCatchPresentationVFX()
    {
        catchSparkleInstance = Instantiate(catchSparklePrefab, VFXposition, Quaternion.identity);
        catchShineInstance = Instantiate(catchShinePrefab, VFXposition, Quaternion.identity);
        catchSparkleInstance.transform.localScale = VFXscale;
        catchShineInstance.transform.localScale = VFXscale;
    }
    void DestroyCatchPresentationVFX()
    {
        if (catchSparkleInstance != null)
        {
            Destroy(catchSparkleInstance);
        }
        else
        {
            Debug.LogWarning("No catch sparkle instance found to destroy.");
        }
        if (catchShineInstance != null)
        {
            Destroy(catchShineInstance);
        }
        else
        {
            Debug.LogWarning("No catch shine instance found to destroy.");
        }
    }
    void SpawnLevelUpPresentationVFX(int newLevel)
    {
        levelUpSparkleInstance = Instantiate(levelUpSparklePrefab, VFXposition, Quaternion.identity);
        levelUpShineInstance = Instantiate(levelUpShinePrefab, VFXposition, Quaternion.identity);
        levelUpSparkleInstance.transform.localScale = VFXscale;
        levelUpShineInstance.transform.localScale = VFXscale;
    }
    void DestroyLevelUpPresentationVFX()
    {
        if (levelUpSparkleInstance != null)
        {
            Destroy(levelUpSparkleInstance);
        }
        else
        {
            Debug.LogWarning("No level up sparkle instance found to destroy.");
        }
        if (levelUpShineInstance != null)
        {
            Destroy(levelUpShineInstance);
        }
        else
        {
            Debug.LogWarning("No level up shine instance found to destroy.");
        }
    }
}