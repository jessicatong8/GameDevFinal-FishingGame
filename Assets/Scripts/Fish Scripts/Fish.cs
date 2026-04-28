using UnityEngine;

public class Fish : MonoBehaviour
{
    public string fishName;
    public float swimmingSpeed = 4f;
    public int rarityWeight;
    public float rarity;
    public float dripThreshold = 0f;

    public bool isActiveFish;


    public float wiggleOnTimer;
    public float wiggleOffTimer;
    public float wiggleStrength;



    [Header("Stats for tension and progress when reeling")]


    public float reelingSpeed = 12f;
    public float tensionDropRate = 5f;
    public float safeZoneCenter = 50f;
    public float safeZoneWidth = 60f;
    public float burstStrength = 0.3f;
    public float tensionEscapeTime = 3f; // how long the player can stay out of the safe tension zone before the fish escapes
}
