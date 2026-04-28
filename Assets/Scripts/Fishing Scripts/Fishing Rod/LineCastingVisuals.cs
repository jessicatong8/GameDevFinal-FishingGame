using UnityEngine;

public class LineCastingVisuals : MonoBehaviour
{
    private VerletFishingLine fishingLine;

    private void Awake()
    {
        ResolveFishingLine();
    }

    // This method is called from an animation event in the cast animation at the frame where the line should be released.
    public void ReleaseCast()
    {
        TriggerCast();
    }

    public void TriggerCast()
    {
        ResolveFishingLine();

        if (fishingLine == null)
        {
            DebugLogger.Instance.LogWarning("LineCastingVisuals.TriggerCast: No VerletFishingLine found, so the cast line cannot move.");
            return;
        }

        fishingLine.TriggerCast();
    }

    public void TriggerReel()
    {
        ResolveFishingLine();

        if (fishingLine == null)
        {
            DebugLogger.Instance.LogWarning("LineCastingVisuals.TriggerReel: No VerletFishingLine found, so the reel line cannot move.");
            return;
        }

        fishingLine.TriggerReel();
    }

    private void ResolveFishingLine()
    {
        if (fishingLine != null)
        {
            return;
        }

        fishingLine = GetComponentInChildren<VerletFishingLine>(true);
    }
}