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

    [Header("VFX Offsets")]
    [SerializeField] private Vector3 VFXposition = new Vector3(-0.150000006f, 3.57999992f, -3.73000002f);
    [SerializeField] private Vector3 VFXscale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Catch Presentation VFX Prefabs")]
    [SerializeField] private GameObject catchSparklePrefab;
    [SerializeField] private GameObject catchShinePrefab;

    [Header("Level Up VFX Prefabs")]
    [SerializeField] private GameObject levelUpSparklePrefab;
    [SerializeField] private GameObject levelUpShinePrefab;
    [SerializeField] private GameObject[] levelUpHats; 

    private GameObject currentHat;
    private GameObject catchSparkleInstance;
    private GameObject catchShineInstance;
    private GameObject levelUpSparkleInstance;
    private GameObject levelUpShineInstance;

    private void OnEnable()
    {
        FishingManager.OnBite += PlayBiteSplash;
        FishingManager.OnCaught += SpawnCatchPresentationVFX;
        FishingManager.OnEscaped += PlayEscapeShake;
        PlayerInputState.MashPerformed += PlayMashSplash;
        PlayerInputState.ConfirmPerformed += DestroyCatchPresentationVFX;
        PlayerInputState.ConfirmPerformed += DestroyLevelUpPresentationVFX;
        FishingManager.OnLevelUp += SpawnLevelUpPresentationVFX; // also play mash splash on level up for that extra satisfying feedback

    }

    private void OnDisable()
    {
        FishingManager.OnBite -= PlayBiteSplash;
        FishingManager.OnCaught -= SpawnCatchPresentationVFX;
        FishingManager.OnEscaped -= PlayEscapeShake;
        PlayerInputState.MashPerformed -= PlayMashSplash;
        PlayerInputState.ConfirmPerformed -= DestroyCatchPresentationVFX;
        PlayerInputState.ConfirmPerformed -= DestroyLevelUpPresentationVFX;
        FishingManager.OnLevelUp -= SpawnLevelUpPresentationVFX;
    }
    void PlayBiteSplash()
    {
        SpawnSplash(biteSplash, 1, bobPoint.position);
    }

    void PlayMashSplash()
    {
        //line range is an oval so we spawn mash effects within the oval using two radii
        float xRadius = LineRangeManager.Instance.xLineRightRange; 
        float zRadius = 4f; 

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(0f, 1f);

        float offsetX = Mathf.Cos(angle) * distance * xRadius;
        float offsetZ = Mathf.Sin(angle) * distance * zRadius;

        Vector3 spawnPos = new Vector3(mashPoint.position.x + offsetX, mashPoint.position.y, mashPoint.position.z + offsetZ);
        
        SpawnSplash(Random.Range(0, 2) == 0 ? mashSplash1 : mashSplash2, 1f, spawnPos);
    }

    void PlayEscapeShake()
    {
        screenShake.TriggerLargeShake(0.8f, 1f);
    }
    void SpawnSplash(GameObject prefab, float scale, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = Vector3.one * scale;
        Destroy(instance, 3f);
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
    void SpawnLevelUpPresentationVFX()
    {
        levelUpSparkleInstance = Instantiate(levelUpSparklePrefab, VFXposition, Quaternion.identity);
        levelUpShineInstance = Instantiate(levelUpShinePrefab, VFXposition, Quaternion.identity);
        levelUpSparkleInstance.transform.localScale = VFXscale;
        levelUpShineInstance.transform.localScale = VFXscale;

        Transform targetAnchor = PlayerCamera.Instance.transform;
        int newLevel = LevelManager.Instance.GetPlayerLevel();
        Vector3 targetPosition = targetAnchor.position + targetAnchor.forward;
        Vector3 localOffset = new Vector3(0f, -0.5f, 2.5f);
        currentHat = levelUpHats[newLevel-2];
        Instantiate(currentHat, targetPosition + targetAnchor.TransformVector(localOffset), Quaternion.identity);
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
        Destroy(currentHat);
    }
}