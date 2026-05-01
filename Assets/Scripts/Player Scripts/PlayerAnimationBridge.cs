using UnityEngine;

public class PlayerAnimationBridge : MonoBehaviour
{
    // Allows animation events to trigger methods in the FishingRig script since they are on different GameObjects.
    private FishingRig fishingRig;

    private void Awake()
    {
        // Find the FishingRig script in children
        fishingRig = GetComponentInChildren<FishingRig>(true);
        // DebugLogger.Instance.LogMethodCall("PlayerAnimationBridge.Awake", fishingRig != null ? "FishingRig found and cached." : "No FishingRig found in children.");
    }
    public void TriggerCast()
    {
        // DebugLogger.Instance.LogMethodCall("PlayerAnimationBridge.TriggerCast", "Called from animation event.");
        fishingRig?.TriggerCast();
    }
    // Not currently being triggered by animation events, but could be in the future if we want to sync the reel animation more closely with the physics.
    public void TriggerReel()   
    {
        // DebugLogger.Instance.LogMethodCall("PlayerAnimationBridge.TriggerReel", "Called from animation event.");
        fishingRig?.TriggerReel();
    }
}