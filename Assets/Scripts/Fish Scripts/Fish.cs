using UnityEngine;

public class Fish : MonoBehaviour
{
    public string fishName;
    public float level;
    public bool isActiveFish;


    public float swimmingSpeed = 4f;
    public float reelingSpeed = 5f; //how fast the fish swims when in reeling state


    // public float wiggleOnTimer;
    // public float wiggleOffTimer;
    // public float wiggleStrength;

    [Header("Reeling - Tension/Progress Settings")]
    // public float maxTension = 100f;
    public float reelingProgressRate = 10f; // how fast the fish gets reeled in when mashing
    public float tensionDropRate = 5f;
    public float safeZoneCenter = 50f;
    public float safeZoneWidth = 50f;
    public float burstStrength = 0.3f;
    public float tensionEscapeTime = 3f; // how long the player can stay out of the safe tension zone before the fish escapes
}
