using Unity.VisualScripting;
using UnityEngine;

public class CastingManager : MonoBehaviour
{
    [Header("Fishing Circle")]
    public Transform detectionPoint; 
    public float detectionRadius;
    public LayerMask fishLayer; 

    public Fish GetFishInArea()
    {
        Collider[] hitFish = Physics.OverlapSphere(detectionPoint.position, detectionRadius, fishLayer);
        
        if (hitFish.Length == 0)
        {
            return null;
        }
        return hitFish[Random.Range(0, hitFish.Length)].GetComponent<Fish>();
        
    }
}
