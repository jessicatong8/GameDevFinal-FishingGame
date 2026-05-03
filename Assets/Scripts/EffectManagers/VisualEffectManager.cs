using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject castSplash; //less necesary 
    public GameObject biteSplash;
    public GameObject mashSplash1;
    public GameObject mashSplash2;

    //change based on andys line logic
    [Header("Spawn Location")]
    public Transform bobPoint;
    public Transform mashPoint;

    [Header("Screen Shake Reference")]
    [SerializeField] private ScreenShake screenShake;

    private void OnEnable()
    {
        FishingManager.OnCast += PlayCastSplash;
        FishingManager.OnBite += PlayBiteSplash;
        FishingManager.OnEscaped += PlayEscapeShake;
        PlayerInputState.MashPerformed += PlayMashSplash;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSplash;
        FishingManager.OnBite -= PlayBiteSplash;
        FishingManager.OnEscaped -= PlayEscapeShake;
        PlayerInputState.MashPerformed -= PlayMashSplash;
    }

    void PlayCastSplash()
    {
        StartCoroutine(DelayedSpawn(castSplash, 3.5f, 0.3f, bobPoint.position));
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

    private IEnumerator DelayedSpawn(GameObject prefab, float delay, float scale, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        SpawnSplash(prefab, scale, position);
    }

    void SpawnSplash(GameObject prefab, float scale, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = Vector3.one * scale;
    }
    // private Vector3 GetActiveFishPosition()
    // {
    //     return FishingManager.Instance.activeFish.transform.position;
    // }
}
