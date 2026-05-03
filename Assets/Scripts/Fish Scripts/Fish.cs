using UnityEngine;

public class Fish : MonoBehaviour
{
    public string fishName;
    public int level;
    public bool isActiveFish;


    public float swimmingSpeed = 4f;
    public float reelingSpeed = 5f; //how fast the fish swims when in reeling state

    [Header("Reeling - Tension/Progress Settings")]
    public float reelingProgressRate = 20f; // how fast the fish gets reeled (progress increases) in when mashing
    public float tensionIncreaseRate = 20f; // how fast the tension increases when mashing
    public float tensionDropRate = 20f; // how fast the tension drops when mashing
    public float safeZoneCenter = 50f;
    public float safeZoneWidth = 50f;
    public float startingTension = 50f;
    public float tensionEscapeTime = 3f; // how long the player can stay out of the safe tension zone before the fish escapes



    // void Start()
    // {
    //     switch (level)
    //     {
    //         case 1:

    //             swimmingSpeed = 4f;
    //             reelingSpeed = 5f;

    //             reelingProgressRate = 15;
    //             tensionIncreaseRate = 20f;
    //             tensionDropRate = 20f;
    //             safeZoneCenter = 50f;
    //             safeZoneWidth = 40f;
    //             burstStrength = 1f;
    //             tensionEscapeTime = 3f;
    //             break;

    //         case 2:
    //             swimmingSpeed = 5f;
    //             reelingSpeed = 7f;

    //             reelingProgressRate = 8;
    //             tensionIncreaseRate = 20f;
    //             tensionDropRate = 30f;
    //             safeZoneCenter = 50f;
    //             safeZoneWidth = 30f;
    //             burstStrength = 2f;
    //             tensionEscapeTime = 2f;
    //             break;

    //         case 3:
    //             swimmingSpeed = 6f;
    //             reelingSpeed = 8f;

    //             reelingProgressRate = 5;
    //             tensionIncreaseRate = 30f;
    //             tensionDropRate = 40f;
    //             safeZoneCenter = 50f;
    //             safeZoneWidth = 20f;
    //             burstStrength = 3f;
    //             tensionEscapeTime = 1f;
    //             break;
    //     }
    // }
}
