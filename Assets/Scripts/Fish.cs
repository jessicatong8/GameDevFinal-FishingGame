using UnityEngine;

public class Fish : MonoBehaviour
{
    public string fishName;
    public float wiggleOnTimer;
    public float wiggleOffTimer;
    public float wiggleStrength;
    public float reelingSpeed;
    public float tensionDropRate;
    public int rarityWeight;
    public float maxTension; //make sure matches ui bar for now

    public float rarity;
    public float dripThreshold;
    public float swimmingSpeed;
}
