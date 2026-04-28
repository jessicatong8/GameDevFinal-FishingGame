using UnityEngine;

public class FishingRig : MonoBehaviour
{
    [Header("Rig Root")]
    [SerializeField] private GameObject fishingRodAssembly;

    [Header("Child Paths From Rig Root")]
    [SerializeField] private string fishingLineChildPath = "Line";
    [SerializeField] private string fishingHookChildPath = "Hook";

    [Header("Cast Anchor From Rig Root")]
    [SerializeField] private string castPointChildPath = "CastPoint";

    private GameObject fishingLineObject;
    private GameObject fishingHookObject;
    private Transform castPointTransform;
    private VerletFishingLine fishingLine;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetActive(bool active)
    {
        fishingRodAssembly?.SetActive(active);
        fishingLineObject?.SetActive(active);
        fishingHookObject?.SetActive(active);
        fishingLine?.SetEquippedFromController(active);
        ApplyCastPointToLine();
    }

    private void ResolveReferences()
    {
        if (fishingRodAssembly == null)
        {
            fishingRodAssembly = gameObject;
        }

        Transform root = fishingRodAssembly.transform;

        Transform lineTransform = string.IsNullOrWhiteSpace(fishingLineChildPath) ? null : root.Find(fishingLineChildPath);
        Transform hookTransform = string.IsNullOrWhiteSpace(fishingHookChildPath) ? null : root.Find(fishingHookChildPath);
        Transform castTransform = string.IsNullOrWhiteSpace(castPointChildPath) ? null : root.Find(castPointChildPath);

        fishingLineObject = lineTransform != null ? lineTransform.gameObject : null;
        fishingHookObject = hookTransform != null ? hookTransform.gameObject : null;
        castPointTransform = castTransform;

        fishingLine = lineTransform != null ? lineTransform.GetComponentInChildren<VerletFishingLine>(true) : null;
        if (fishingLine == null)
        {
            fishingLine = fishingRodAssembly.GetComponentInChildren<VerletFishingLine>(true);
            fishingLineObject = fishingLine != null ? fishingLine.gameObject : fishingLineObject;
        }

        ApplyCastPointToLine();
    }

    private void ApplyCastPointToLine()
    {
        if (fishingLine == null || castPointTransform == null)
        {
            return;
        }

        fishingLine.StartPoint = castPointTransform;
    }
}
