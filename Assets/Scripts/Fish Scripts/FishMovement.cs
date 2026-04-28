using UnityEngine;

public class FishMovement : MonoBehaviour
{

    private Vector3 position;
    private Vector3 targetPosition;

    private float swimmingSpeed;

    private float xLeftBoundary = -12f;
    private float xRightBoundary = 12f;

    private float xLineLeftWarningRange;
    private float xLineRightWarningRange;
    private float xLineLeftRange;
    private float xLineRightRange;

    private float baseSwimmingSpeed;

    [SerializeField] private float idleHeight = 0f;
    [SerializeField] private float reelingHeight = 1f;


    [SerializeField] private float idleSpeedVariation = 0.2f;
    [SerializeField] private float reelingSpeedVariation = 0.5f;
    [SerializeField] private float speedNoiseFrequency = 0.8f;
    private float speedNoiseOffset;

    [SerializeField] private float idleWobbleAmplitude = 0.02f;
    [SerializeField] private float reelingWobbleAmplitude = 0.008f;

    [SerializeField] private float wobbleFrequency = 2f;
    private float wobbleOffset;
    private Vector3 basePosition;

    private float arrivalThreshold = 0.1f;
    private int direction = 1; // 1 for right, -1 for left


    private enum FishingGameState
    {
        Idle,
        Reeling,
        // CatchPresentation
    }
    private FishingGameState currentFishingGameState = FishingGameState.Idle;



    void Start()
    {
        xLineLeftWarningRange = LineRangeManager.Instance.xLineLeftWarningRange;
        xLineRightWarningRange = LineRangeManager.Instance.xLineRightWarningRange;
        xLineLeftRange = LineRangeManager.Instance.xLineLeftRange;
        xLineRightRange = LineRangeManager.Instance.xLineRightRange;

        position = new Vector3(transform.position.x, idleHeight, transform.position.z);
        transform.position = position;

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        wobbleOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition(position);
        // DebugLogger.Instance.Log("Initial Position: " + position);
    }

    private void OnEnable()
    {
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnReturnToIdle += HandleResetToIdle;
        PlayerInputState.Instance.ReelLeftPerformed += TurnLeft;
        PlayerInputState.Instance.ReelRightPerformed += TurnRight;
    }

    private void OnDisable()
    {
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnReturnToIdle -= HandleResetToIdle;
        PlayerInputState.Instance.ReelLeftPerformed -= TurnLeft;
        PlayerInputState.Instance.ReelRightPerformed -= TurnRight;
    }


    private void Update()
    {
        UpdateSwimmingSpeed();
        SwimTowardTarget(currentFishingGameState);

        // switch (currentFishingGameState)
        // {
        //     case FishingGameState.Idle:
        //         HandleIdleMovement();
        //         break;

        //     case FishingGameState.Reeling:
        //         HandleReelingMovment();
        //         // HandleReeling(); // Now handled by ProgressManager and TensionManager
        //         break;
        // }
    }



    private void HandleHooked()
    {
        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log("Not the active fish, ignoring input.");
            return;
        }

        currentFishingGameState = FishingGameState.Reeling;
        position = new Vector3(0, reelingHeight, transform.position.z);
        transform.position = position;

        baseSwimmingSpeed = GetComponent<Fish>().reelingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        ReelingSetTargetPosition(position);
    }

    private void HandleResetToIdle()
    {
        currentFishingGameState = FishingGameState.Idle;
        position = new Vector3(transform.position.x, idleHeight, transform.position.z);
        transform.position = position;

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed; // reset the base speed when fight is over
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition(position);
    }

    // private void HandleIdleMovement()
    // {
    //     SwimTowardTarget(currentFishingGameState);
    // }

    // private void HandleReelingMovment()
    // {
    //     SwimTowardTarget(currentFishingGameState);
    // }

    public Vector3 IdleSetTargetPosition(Vector3 position)
    {
        if (position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(UnityEngine.Random.Range(xLineRightRange, xRightBoundary), idleHeight, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(UnityEngine.Random.Range(xLeftBoundary, xLineLeftRange), idleHeight, position.z);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        // Debug.Log("Target Position: " + targetPosition);
        return targetPosition;
    }

    public Vector3 ReelingSetTargetPosition(Vector3 position)
    {
        if (position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(UnityEngine.Random.Range(xLineRightWarningRange - 1, xLineRightRange + 5), reelingHeight, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(UnityEngine.Random.Range(xLineLeftRange - 5, xLineLeftWarningRange + 1), reelingHeight, position.z);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        // Debug.Log("Target Position: " + targetPosition);
        return targetPosition;
    }

    private void SwimTowardTarget(FishingGameState state)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            transform.position = targetPosition;
            if (state == FishingGameState.Idle)
            {
                IdleSetTargetPosition(transform.position);
            }
            else if (state == FishingGameState.Reeling)
            {
                ReelingSetTargetPosition(transform.position);
            }
            return;
        }
        // transform.LookAt(targetPosition);
        basePosition = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

        // Written using AI: Apply subtle vertical wobble
        float wobbleAmplitude = state == FishingGameState.Idle ? idleWobbleAmplitude : reelingWobbleAmplitude;
        float wobble = Mathf.Sin(Time.time * wobbleFrequency + wobbleOffset) * wobbleAmplitude;
        transform.position = basePosition + Vector3.forward * wobble;
    }

    // Written using AI: Adding noise to the swimming speed
    private void UpdateSwimmingSpeed()
    {
        float variation = currentFishingGameState == FishingGameState.Reeling ? reelingSpeedVariation : idleSpeedVariation;
        float noise = Mathf.PerlinNoise(Time.time * speedNoiseFrequency + speedNoiseOffset, 0f);
        float speedMultiplier = Mathf.Lerp(1f - variation, 1f + variation, noise);
        swimmingSpeed = baseSwimmingSpeed * speedMultiplier;
    }
    // Written using AI: Adding noise to the swimming speed
    private void ApplySpeedVariation()
    {
        float variation = currentFishingGameState == FishingGameState.Reeling ? reelingSpeedVariation : idleSpeedVariation;
        swimmingSpeed = GetRandomSpeed(baseSwimmingSpeed, variation);
    }

    // Written using AI: Adding noise to the swimming speed        
    private float GetRandomSpeed(float baseSpeed, float variation)
    {
        return baseSpeed * Random.Range(1f - variation, 1f + variation);
    }


    public void TurnLeft()
    {
        // Debug.Log("Reel Left Input Received");
        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log("No active fish, ignoring input.");
            return;
        }


        if (direction != -1)
        {
            direction = -1;
            ReelingSetTargetPosition(transform.position);
            // transform.LookAt(targetPosition);
        }
    }

    public void TurnRight()
    {
        // Debug.Log("Reel Right Input Received");

        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log("No active fish, ignoring input.");
            return;
        }

        if (direction != 1)
        {
            direction = 1;
            ReelingSetTargetPosition(transform.position);
            // transform.LookAt(targetPosition);
        }
    }
    public bool IsInInnerLineRange()
    {
        return transform.position.x > xLineLeftWarningRange && transform.position.x < xLineRightWarningRange;
    }

    public bool IsInLeftWarningRange()
    {

        return transform.position.x <= xLineLeftWarningRange && transform.position.x >= xLineLeftRange;
    }
    public bool IsInRightWarningRange()
    {
        return transform.position.x >= xLineRightWarningRange && transform.position.x <= xLineRightRange;
    }

    public bool IsOutOfLineRange()
    {
        return transform.position.x < xLineLeftRange || transform.position.x > xLineRightRange;
    }
}
