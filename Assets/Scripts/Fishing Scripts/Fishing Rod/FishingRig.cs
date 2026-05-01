using UnityEngine;

public class FishingRig : MonoBehaviour
{
    [Header("Rig Root")]
    [SerializeField] private GameObject fishingRodAssembly;

    [Header("Child Paths From Rig Root")]
    [SerializeField] private string fishingLineChildPath = "Line";
    [SerializeField] private string fishingHookChildPath = "Hook";
    [SerializeField] private string castPointChildPath = "CastPoint";

    private GameObject fishingLineObject;
    private GameObject fishingHookObject;
    private Transform castPointTransform;
    private VerletFishingLine fishingLine;

    private void Awake()
    {
        ResolveReferences();
    }
    private void ResolveReferences()
    {
        if (fishingRodAssembly == null) fishingRodAssembly = gameObject;
        Transform root = fishingRodAssembly.transform;

        // Find child objects by string paths
        Transform lineTransform = root.Find(fishingLineChildPath);
        Transform hookTransform = root.Find(fishingHookChildPath);
        castPointTransform = root.Find(castPointChildPath);

        fishingLineObject = lineTransform?.gameObject;
        fishingHookObject = hookTransform?.gameObject;

        // Cache the physics component
        fishingLine = fishingRodAssembly.GetComponentInChildren<VerletFishingLine>(true);
        
        ApplyCastPointToLine();
    }
    private void ApplyCastPointToLine()
    {
        if (fishingLine != null && castPointTransform != null)
        {
            fishingLine.StartPoint = castPointTransform; // Set physics start anchor[cite: 12, 14]
        }
    }
    // Toggles the entire rig active/inactive and resets line physics.
    public void SetActive(bool active)
    {
        fishingRodAssembly?.SetActive(active);
        fishingLineObject?.SetActive(active);
        fishingHookObject?.SetActive(active);
        fishingLine?.SetEquippedFromController(active);
        ApplyCastPointToLine();
    }
    // ANIMATION EVENT: Triggers the physics cast flight.
    public void TriggerCast()
    {
        if (fishingLine != null)
        {
            fishingLine.TriggerCast(); // Triggers the Verlet flight
        }
    }
    // ANIMATION EVENT: Shrinks line length back to start.
    public void TriggerReel()
    {
        if (fishingLine != null)
        {
            fishingLine.TriggerReel(); // Triggers the reel length change
        }
    }
}
// using UnityEngine;

// public class FishingRig : MonoBehaviour
// {
//     [Header("Rig Root")]
//     [SerializeField] private GameObject fishingRodAssembly;
//     [SerializeField] private GameObject fishingLineObject;
//     [SerializeField] private GameObject fishingHookObject;
//     [SerializeField] private Transform castPointTransform;
//     [SerializeField] private VerletFishingLine fishingLine;

//     private void Awake()
//     {
//         ResolveReferences();
//     }
//     private void ResolveReferences()
//     {
//         if (fishingRodAssembly == null) fishingRodAssembly = gameObject;
//         Transform root = fishingRodAssembly.transform;

//         // Find child objects by string paths
//         if (castPointTransform == null)
//         {
//             DebugLogger.Instance.Log("FishingRig.ResolveReferences: Attempting to find CastPoint child transform under fishingRodAssembly.");
//             Transform castPoint = root.Find("CastPoint");
//             if (castPoint == null)
//             {
//                 DebugLogger.Instance.LogWarning("FishingRig.ResolveReferences: No child named 'CastPoint' found under fishingRodAssembly.");
//             }
//         }
//         Transform lineTransform = fishingLineObject.GetComponent<Transform>();
//         Transform hookTransform = fishingHookObject.GetComponent<Transform>();

//         fishingLineObject = lineTransform?.gameObject;
//         fishingHookObject = hookTransform?.gameObject;

//         // Cache the physics component
//         fishingLine = fishingRodAssembly.GetComponentInChildren<VerletFishingLine>(true);
        
//         ApplyCastPointToLine();
//     }

//     /// <summary>
//     /// Toggles the entire rig active/inactive and resets line physics.
//     /// </summary>
//     public void SetActive(bool active)
//     {
//         fishingRodAssembly?.SetActive(active);
//         fishingLineObject?.SetActive(active);
//         fishingHookObject?.SetActive(active);
        
//         // Tells the Verlet system if it should simulate[cite: 12, 14]
//         fishingLine?.SetEquippedFromController(active);
//         ApplyCastPointToLine();
//     }

//     /// <summary>
//     /// ROUTED FROM ANIMATION EVENT: Triggers the physics cast flight.
//     /// </summary>
//     public void TriggerCast()
//     {
//         if (fishingLine != null)
//         {
//             fishingLine.TriggerCast(); // Triggers the Verlet flight[cite: 12, 13]
//         }
//     }

//     /// <summary>
//     /// ROUTED FROM ANIMATION EVENT: Shrinks line length back to start.
//     /// </summary>
//     public void TriggerReel()
//     {
//         if (fishingLine != null)
//         {
//             fishingLine.TriggerReel(); // Triggers the reel length change[cite: 12, 13]
//         }
//     }
//     private void ApplyCastPointToLine()
//     {
//         if (fishingLine != null && castPointTransform != null)
//         {
//             fishingLine.StartPoint = castPointTransform; // Set physics start anchor[cite: 12, 14]
//         }
//     }
// }