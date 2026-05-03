using UnityEngine;
using UnityEngine.InputSystem;

public class FishMovement : MonoBehaviour
{

    private Vector3 position;
    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private float swimmingSpeed;

    private float xIdleBoundaryDistance = 25f;
    // private float xDistanceRightIdleBoundary = 25f;

    // private float xLineLeftWarningRange;
    // private float xLineRightWarningRange;
    // private float xLineLeftRange;
    // private float xLineRightRange;

    private float baseSwimmingSpeed;


    [SerializeField] private float idleHeight = -1f;
    [SerializeField] private float reelingHeight = 0f;


    [SerializeField] private float idleSpeedVariation = 0.2f;
    [SerializeField] private float reelingSpeedVariation = 0.5f;
    [SerializeField] private float speedNoiseFrequency = 0.8f;
    private float speedNoiseOffset;

    [SerializeField] private float idleWobbleAmplitude = 0.01f;
    [SerializeField] private float reelingWobbleAmplitude = 0.008f;

    [SerializeField] private float wobbleFrequency = 2f;
    private float wobbleOffset;
    private Vector3 basePosition;
    private float arrivalThreshold = 0.1f;
    private int direction = 1; // 1 for right, -1 for left
    [Header("Fish Placement")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.5f, 2.5f);
    [SerializeField] private float rotationSpeed = 40f;
    private void OnEnable()
    {
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnReturnToGameplay += HandleResetToGameplay;

        PlayerInputState.ReelLeftPerformed += TurnLeft;
        PlayerInputState.ReelRightPerformed += TurnRight;
        PlayerInputState.ConfirmCatchPerformed += HandleCatchConfirmation;
    }

    private void OnDisable()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnReturnToGameplay -= HandleResetToGameplay;
        PlayerInputState.ReelLeftPerformed -= TurnLeft;
        PlayerInputState.ReelRightPerformed -= TurnRight;
        PlayerInputState.ConfirmCatchPerformed -= HandleCatchConfirmation;
    }
    // Public methods that check fish position, used by LineRangeManager
    public bool IsInInnerLineRange()
    {
        return transform.position.x > LineRangeManager.Instance.xLineLeftWarningRange && transform.position.x < LineRangeManager.Instance.xLineRightWarningRange;
    }
    public bool IsInLeftWarningRange()
    {

        return transform.position.x <= LineRangeManager.Instance.xLineLeftWarningRange && transform.position.x >= LineRangeManager.Instance.xLineLeftRange;
    }
    public bool IsInRightWarningRange()
    {
        return transform.position.x >= LineRangeManager.Instance.xLineRightWarningRange && transform.position.x <= LineRangeManager.Instance.xLineRightRange;
    }
    public bool IsOutOfLineRange()
    {
        return transform.position.x < LineRangeManager.Instance.xLineLeftRange || transform.position.x > LineRangeManager.Instance.xLineRightRange;
    }


    void Start()
    {
        // xLineLeftWarningRange = LineRangeManager.Instance.xLineLeftWarningRange;
        // xLineRightWarningRange = LineRangeManager.Instance.xLineRightWarningRange;
        // xLineLeftRange = LineRangeManager.Instance.xLineLeftRange;
        // xLineRightRange = LineRangeManager.Instance.xLineRightRange;

        // Set fish to idle movement on start
        position = new Vector3(transform.position.x, idleHeight, transform.position.z);
        transform.position = position;
        originalPosition = position;

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        wobbleOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition();
    }

    private void Update()
    {
        switch (FishingManager.Instance.CurrentFishingGameState)
        {
            case FishingManager.FishingGameState.Gameplay:
                break;
            case FishingManager.FishingGameState.HookWindow:
                break;
            case FishingManager.FishingGameState.Reeling:
                break;

        }
        if (GetComponent<Fish>().isActiveFish && FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
        {
            RotateFish();
            return;
        }
        UpdateSwimmingSpeed();
        // IdleSetTargetPosition();
        SwimTowardTarget();
    }

    private void IdleSetTargetPosition()
    {
        SetTargetPosition(idleHeight, LineRangeManager.Instance.xLineRightRange, xIdleBoundaryDistance); //Idle

    }
    private void ReelingSetTargetPosition()
    {
        SetTargetPosition(reelingHeight, LineRangeManager.Instance.xLineRightRange, LineRangeManager.Instance.xLineRightRange + 5); //Reeling
    }


    // public Vector3 IdleSetTargetPosition(Vector3 position)
    // {
    //     if (position.x <= 0)
    //     {
    //         direction = 1; // Move right
    //         targetPosition = new Vector3(UnityEngine.Random.Range(LineRangeManager.Instance.xLineRightRange, xIdleBoundaryDistance), idleHeight, position.z);
    //     }
    //     else if (position.x > 0)
    //     {
    //         direction = -1; // Move left
    //         targetPosition = new Vector3(UnityEngine.Random.Range(-xIdleBoundaryDistance, LineRangeManager.Instance.xLineLeftRange), idleHeight, position.z);
    //     }
    //     transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

    //     // Debug.Log("Target Position: " + targetPosition);
    //     return targetPosition;
    // }
    // public Vector3 ReelingSetTargetPosition(Vector3 position)
    // {
    //     if (position.x <= 0)
    //     {
    //         direction = 1; // Move right
    //         targetPosition = new Vector3(UnityEngine.Random.Range(LineRangeManager.Instance.xLineRightRange, LineRangeManager.Instance.xLineRightRange + 5), reelingHeight, position.z);
    //     }
    //     else if (position.x > 0)
    //     {
    //         direction = -1; // Move left
    //         targetPosition = new Vector3(UnityEngine.Random.Range(LineRangeManager.Instance.xLineLeftRange - 5, LineRangeManager.Instance.xLineLeftRange), reelingHeight, position.z);
    //     }
    //     transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

    //     // Debug.Log("Target Position: " + targetPosition);
    //     return targetPosition;
    // }


    public Vector3 SetTargetPosition(float targetPositionY, float xDistanceStart, float xDistanceEnd)
    {
        if (transform.position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(Random.Range(xDistanceStart, xDistanceEnd), targetPositionY, position.z);
        }
        else if (transform.position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(Random.Range(-xDistanceEnd, -xDistanceStart), targetPositionY, position.z);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        // Debug.Log("Target Position: " + targetPosition);
        return targetPosition;
    }
    private void SwimTowardTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            transform.position = targetPosition;
            if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
            {
                ReelingSetTargetPosition();
            }
            else
            {
                IdleSetTargetPosition();
            }
            return;
        }
        transform.LookAt(targetPosition);
        basePosition = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

        ApplyVerticalWobble();

    }
    private void ApplyVerticalWobble()
    {
        // Written using AI: Apply subtle vertical wobble
        float wobbleAmplitude = FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Gameplay ? idleWobbleAmplitude : reelingWobbleAmplitude;
        float wobble = Mathf.Sin(Time.time * wobbleFrequency + wobbleOffset) * wobbleAmplitude;
        transform.position = basePosition + Vector3.forward * wobble;

    }


    // Written using AI: Adding noise to the swimming speed
    private void UpdateSwimmingSpeed()
    {
        float variation = FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Reeling ? reelingSpeedVariation : idleSpeedVariation;
        float noise = Mathf.PerlinNoise(Time.time * speedNoiseFrequency + speedNoiseOffset, 0f);
        float speedMultiplier = Mathf.Lerp(1f - variation, 1f + variation, noise);
        swimmingSpeed = baseSwimmingSpeed * speedMultiplier;
    }
    // Written using AI: Adding noise to the swimming speed
    private void ApplySpeedVariation()
    {
        float variation = FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Reeling ? reelingSpeedVariation : idleSpeedVariation;
        swimmingSpeed = GetRandomSpeed(baseSwimmingSpeed, variation);
    }

    // Written using AI: Adding noise to the swimming speed        
    private float GetRandomSpeed(float baseSpeed, float variation)
    {
        return baseSpeed * Random.Range(1f - variation, 1f + variation);
    }
    public void TurnLeft()
    {
        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log(GetComponent<Fish>().fishName + " is not the active fish, ignoring input.");
            return;
        }

        if (direction != -1)
        {
            // Debug.Log("Turning Left");
            direction = -1;
            ReelingSetTargetPosition();
            transform.LookAt(targetPosition);
        }
    }
    public void TurnRight()
    {
        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log(GetComponent<Fish>().fishName + " is not the active fish, ignoring input.");
            return;
        }

        if (direction != 1)
        {
            direction = 1;
            ReelingSetTargetPosition();
            transform.LookAt(targetPosition);
        }
    }
    private void HandleBite()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        // FishingManager.Instance.CurrentFishingGameState = FishingManager.FishingGameState.Reeling;
        position = new Vector3(0, reelingHeight, 5f);
        transform.position = position;
        transform.eulerAngles = new Vector3(-10, transform.eulerAngles.y, transform.eulerAngles.z); //tilt fish up
        ReelingSetTargetPosition();

    }
    private void HandleHooked()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);

        // FishingManager.Instance.CurrentFishingGameState = FishingManager.FishingGameState.Reeling;
        // position = new Vector3(0, reelingHeight, 5f);
        // transform.position = position;

        baseSwimmingSpeed = GetComponent<Fish>().reelingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        ReelingSetTargetPosition();
    }
    private void HandleCaught()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        PlaceFishInFrontOfCamera();
    }
    private void HandleResetToGameplay()
    {
        // currentFishingGameState = FishingGameState.Idle;
        position = originalPosition;
        transform.eulerAngles = new Vector3(0, direction * 90f, 0);
        transform.position = position;

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed; // reset the base speed
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition();
    }
    private void HandleCatchConfirmation()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        FishingManager.Instance.ReturnToGameplay("FishCaughtConfirmed");
    }
    private void PlaceFishInFrontOfCamera()
    {
        Transform targetAnchor = PlayerCamera.Instance.transform;
        Vector3 targetPosition = targetAnchor.position + targetAnchor.forward;
        transform.position = targetPosition + targetAnchor.TransformVector(localOffset);
        transform.LookAt(PlayerCamera.Instance.transform.position);
    }
    private void RotateFish()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

}
