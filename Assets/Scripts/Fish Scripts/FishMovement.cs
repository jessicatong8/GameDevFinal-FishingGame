using UnityEngine;

public class FishMovement : MonoBehaviour
{

    private Vector3 position;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 originalScale;

    private float swimmingSpeed;

    [SerializeField] private float xLeftBoundary = -12f;
    [SerializeField] private float xRightBoundary = 12f;

    private float xLineLeftWarningRange;
    private float xLineRightWarningRange;
    private float xLineLeftRange;
    private float xLineRightRange;

    private float baseSwimmingSpeed;
    private bool isActiveFish;

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
    private int direction = 1; // 1 for right, -1 for left\
    private PlayerCamera playerCamera;
    private LineRangeManager lineRangeManager;
    [Header("Fish Placement")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.5f, 2.5f);
    [SerializeField] private float rotationSpeed = 40f;


    private void OnEnable()
    {
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHooked;
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnEscaped += HandleResetToGameplay;


        PlayerInputState.ReelLeftPerformed += TurnLeft;
        PlayerInputState.ReelRightPerformed += TurnRight;

        PlayerInputState.CatchConfirmPerformed += HandleCatchConfirmation;
        PlayerInputState.AbortPerformed += HandleResetToGameplay;

    }

    private void OnDisable()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHooked;
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnEscaped -= HandleResetToGameplay;

        PlayerInputState.ReelLeftPerformed -= TurnLeft;
        PlayerInputState.ReelRightPerformed -= TurnRight;

        PlayerInputState.AbortPerformed += HandleResetToGameplay;
        PlayerInputState.CatchConfirmPerformed -= HandleCatchConfirmation;
    }
    void Start()
    {
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        lineRangeManager = FindFirstObjectByType<LineRangeManager>();
        if (lineRangeManager != null)
        {
            xLineLeftWarningRange = lineRangeManager.xLineLeftWarningRange;
            xLineRightWarningRange = lineRangeManager.xLineRightWarningRange;
            xLineLeftRange = lineRangeManager.xLineLeftRange;
            xLineRightRange = lineRangeManager.xLineRightRange;
        }

        position = new Vector3(transform.position.x, idleHeight, transform.position.z);
        transform.position = position;
        originalPosition = position;

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        wobbleOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition(position);
    }

    private void Update()
    {
        if (GetComponent<Fish>().isActiveFish && FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
        {
            RotateFish();
            return;
        }
        UpdateSwimmingSpeed();
        SwimTowardTarget(FishingManager.Instance.CurrentFishingGameState);
    }

    public Vector3 IdleSetTargetPosition(Vector3 position)
    {
        if (position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(Random.Range(xLineRightRange, xRightBoundary), idleHeight, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(Random.Range(xLeftBoundary, xLineLeftRange), idleHeight, position.z);
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
            targetPosition = new Vector3(Random.Range(xLineRightRange, xLineRightRange + 5), reelingHeight, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(Random.Range(xLineLeftRange - 5, xLineLeftRange), reelingHeight, position.z);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        return targetPosition;
    }

    // Written Using AI
    private void SwimTowardTarget(FishingManager.FishingGameState state)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            transform.position = targetPosition;
            if (GetComponent<Fish>().isActiveFish && (state == FishingManager.FishingGameState.Reeling || state == FishingManager.FishingGameState.HookWindow))
            {
                ReelingSetTargetPosition(transform.position);
            }
            else
            {
                IdleSetTargetPosition(transform.position);
            }
            return;
        }
        transform.LookAt(targetPosition);
        basePosition = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

        // Written using AI: Apply subtle vertical wobble
        float wobbleAmplitude = state == FishingManager.FishingGameState.Gameplay ? idleWobbleAmplitude : reelingWobbleAmplitude;
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
            return;
        }

        if (direction != -1)
        {
            // Debug.Log("Turning Left");
            direction = -1;
            ReelingSetTargetPosition(transform.position);
            transform.LookAt(targetPosition);
        }
    }
    public void TurnRight()
    {
        if (!GetComponent<Fish>().isActiveFish)
        {
            return;
        }

        if (direction != 1)
        {
            direction = 1;
            ReelingSetTargetPosition(transform.position);
            transform.LookAt(targetPosition);
        }
    }
    private void HandleBite()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        isActiveFish = true;

        position = new Vector3(0, reelingHeight, 5f);
        transform.position = position;
        transform.eulerAngles = new Vector3(-10, transform.eulerAngles.y, transform.eulerAngles.z); //tilt fish up
        ReelingSetTargetPosition(position);

    }
    private void HandleHooked()
    {
        if (!isActiveFish) return;

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);

        baseSwimmingSpeed = GetComponent<Fish>().reelingSpeed;
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        ReelingSetTargetPosition(position);
    }
    private void HandleCaught()
    {
        if (!isActiveFish) return;
        PlaceFishInFrontOfCamera();
    }

    // fish gradually transitions back to idle speed and idle hieght under the water
    private void HandleResetToGameplay()
    {
        if (!isActiveFish) return;
        isActiveFish = false;

        // Debug.Log("FM: HandleResetToGameplay");

        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed; // reset the base speed
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition(position);

    }

    // fish immediately teleports back to its original idle position under the water
    private void HandleCatchConfirmation()
    {
        if (!isActiveFish) return;
        isActiveFish = false;

        position = originalPosition;
        transform.eulerAngles = new Vector3(0, direction * 90f, 0);
        transform.position = position;
        transform.localScale = originalScale;

        // Debug.Log("FM: HandleCatchCofirmed" + transform.position);


        baseSwimmingSpeed = GetComponent<Fish>().swimmingSpeed; // reset the base speed
        speedNoiseOffset = Random.Range(0f, 100f);
        ApplySpeedVariation();
        IdleSetTargetPosition(position);
    }
    private void PlaceFishInFrontOfCamera()
    {
        Transform targetAnchor = playerCamera.transform;
        Vector3 targetPosition = targetAnchor.position + targetAnchor.forward;
        originalScale = transform.localScale;
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        transform.position = targetPosition + targetAnchor.TransformVector(localOffset);
        transform.LookAt(playerCamera.transform.position);
    }
    private void RotateFish()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
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
