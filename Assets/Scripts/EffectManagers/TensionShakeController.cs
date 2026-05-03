using UnityEngine;

public class TensionShakeController : MonoBehaviour
{
    [SerializeField] private TensionManager tensionManager;
    [SerializeField] private ScreenShake screenShake;
    [SerializeField] private float shakeIntensity = 0.05f;

    void Update()
    {
        //if we are reeling and outside of the safe zone we should shake
        bool shake = (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Reeling) && !tensionManager.IsInSafeZone();

        screenShake.SetConstantShake(shake, shakeIntensity);
    }
}
