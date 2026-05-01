using UnityEngine;

public class VisualEffectManager : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject biteSplash;

    [Header("Spawn Location")]
    public Transform waterSurfacePoint;

    private void OnEnable()
    {
        FishingManager.OnBite += SpawnSplash(biteSplash);
        FishingManager.OnHook += PlayHookSound;
        FishingManager.OnCaught += PlayCatchSound;
        FishingManager.OnEscaped += PlayEscapeSound;
        PlayerInputState.MashPerformed += PlayReelingSound;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSequence;
        FishingManager.OnBite -= PlayBiteSound;
        FishingManager.OnHook -= PlayHookSound;
        FishingManager.OnCaught -= PlayCatchSound;
        FishingManager.OnEscaped -= PlayEscapeSound;
        PlayerInputState.MashPerformed -= PlayReelingSound;
    }

    void SpawnSplash(GameObject prefab)
    {
        Instantiate(prefab, waterSurfacePoint.position, Quaternion.identity);
    }
}
