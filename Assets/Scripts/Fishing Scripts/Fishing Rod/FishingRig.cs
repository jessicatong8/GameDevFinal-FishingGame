using UnityEngine;

public class FishingRig : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private VerletFishingLine fishingLine;
    [SerializeField] private Transform lineStartPoint; // The tip of the physical rod
    [SerializeField] private GameObject fishingHook;   // The physical hook/bobber
    [SerializeField] private GameObject bobVFX;        // Your water ripple/bobbing VFX

    [Header("Targeting")]
    [Tooltip("If assigned, the line will always land here. If null, uses random distance.")]
    [SerializeField] private Transform landingTarget;

    private void Awake()
    {
        if (fishingLine == null) 
        {
            fishingLine = GetComponentInChildren<VerletFishingLine>(true);
        }
        if (fishingLine != null)
        {
            if (lineStartPoint != null) fishingLine.StartPoint = lineStartPoint;
            if (fishingHook != null) fishingLine.EndPoint = fishingHook.transform;
        }
    }
    // Toggles the entire rig active/inactive and resets line physics.
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        fishingLine?.SetEquippedFromController(active);
    }

    // ANIMATION EVENT: Triggers the physics cast flight.
    public void TriggerCast()
    {
        if (landingTarget != null)
        {
            fishingLine.TriggerCast(landingTarget.position);        // If we have a target transform, pass its position.
        }
        else
        {
            fishingLine.TriggerCast(null);                          // No target, use default behavior (random distance)
        }
    }
    // ANIMATION EVENT: Shrinks line length back to start.
    public void TriggerReel()
    {
        fishingLine.TriggerReel();                                  // Triggers the reel length change
    }
}