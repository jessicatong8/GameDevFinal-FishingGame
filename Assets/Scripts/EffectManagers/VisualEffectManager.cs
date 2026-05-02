using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject castSplash;
    public GameObject biteSplash;
    public GameObject mashSplash1;
    public GameObject mashSplash2;
    public GameObject caughtSplash;
    public GameObject escapedSplash;


    [Header("Spawn Location")]
    public Transform bobPoint;
    public Transform mashPoint;

    private void OnEnable()
    {
        FishingManager.OnCast += PlayCastSplash;
        FishingManager.OnBite += PlayBiteSplash;
        PlayerInputState.MashPerformed += PlayMashSplash;
        FishingManager.OnCaught += PlayCaughtSplash;
        FishingManager.OnEscaped += PlayEscapedSplash;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSplash;
        FishingManager.OnBite -= PlayBiteSplash;
        PlayerInputState.MashPerformed -= PlayMashSplash;
        FishingManager.OnCaught -= PlayCaughtSplash;
        FishingManager.OnEscaped -= PlayEscapedSplash;
    }

    void PlayCastSplash()
    {
        StartCoroutine(DelayedSpawn(castSplash, 1.2f, 0.3f, bobPoint.position));
    }
    
    void PlayBiteSplash()
    {
        SpawnSplash(biteSplash, 1, bobPoint.position);
    }

    void PlayMashSplash()
    {
        //Vector2 randomPositionInCircle = Random.insideUnitCircle * radius;
        //Vector3 spawnPosition = waterSurfacePoint.position + new Vector3(randomPositionInCircle.x, 0, randomPositionInCircle.y);
        SpawnSplash(Random.Range(0, 2) == 0 ? mashSplash1 : mashSplash2, 0.8f, mashPoint.position);
    }

    void PlayCaughtSplash()
    {
        //SpawnSplash(caughtSplash, 1, waterSurfacePoint.position);
    }
    void PlayEscapedSplash()
    {
        //SpawnSplash(escapedSplash, 0.5f, waterSurfacePoint.position);
    }

    private IEnumerator DelayedSpawn(GameObject prefab, float delay, float scale, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        SpawnSplash(prefab, scale, position);
    }

    void SpawnSplash(GameObject prefab, float scale, Vector3 position)
    {
        //GameObject instance = 
        Instantiate(prefab, position, Quaternion.identity);
        //instance.transform.localScale = Vector3.one * scale;
    }
}
