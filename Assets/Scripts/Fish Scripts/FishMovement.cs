using System;
using System.Collections;
using UnityEngine;

public class FishMovement : MonoBehaviour
{

    [SerializeField] private LineRangeManager lineRangeManager;
    [SerializeField] private PlayerInputState inputState;


    public static event Action LeavingLineRange;
    public static event Action EnteringLineRange;

    public FishingManager fishManager;
    private Animator animator;
    private Vector3 position;
    private Vector3 targetPosition;
    private float swimmingSpeed;

    // will likely have to adjust these based on the camera view 
    private float xLeftBoundary = -15f;
    private float xRightBoundary = 15f;

    private float xLineLeftWarningRange;
    private float xLineRightWarningRange;
    private float xLineLeftRange;
    private float xLineRightRange;

    public float arrivalThreshold = 0.1f;
    private int direction = 1; // 1 for right, -1 for left
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xLineLeftWarningRange = lineRangeManager.xLineLeftWarningRange;
        xLineRightWarningRange = lineRangeManager.xLineRightWarningRange;
        xLineLeftRange = lineRangeManager.xLineLeftRange;
        xLineRightRange = lineRangeManager.xLineRightRange;

        // FishingManager.OnCast += HandleCast;
        // FishingManager.OnBite += HandleBite;
        // FishingManager.OnReelingActive += HandleReelingActive;
        // FishingManager.OnReelingInactive += HandleReelingInactive;
        // FishingManager.OnCaught += HandleCaught;
        // FishingManager.OnLineBreak += HandleLineBreak;
        // FishingManager.OnWiggle += HandleWiggleStart;
        // FishingManager.OffWiggle += HandleWiggleEnd;
        position = transform.position;
        swimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        SetTargetPosition(position);
        // DebugLogger.Instance.Log("Initial Position: " + position);

    }

    private void OnEnable()
    {
        if (inputState != null)
        {
            inputState.ReelLeftPerformed += TurnLeft;
            inputState.ReelRightPerformed += TurnRight;
        }
    }

    private void OnDisable()
    {
        if (inputState != null)
        {
            inputState.ReelLeftPerformed -= TurnLeft;
            inputState.ReelRightPerformed -= TurnRight;
        }
    }
    // Update is called once per frame
    void Update()
    {
        SwimTowardTarget();
    }

    public Vector3 SetTargetPosition(Vector3 position)
    {
        if (position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(UnityEngine.Random.Range(xLineRightRange, xRightBoundary), position.y, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(UnityEngine.Random.Range(xLeftBoundary, xLineLeftRange), position.y, position.z);
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
            SetTargetPosition(transform.position);
            return;
        }
        transform.LookAt(targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

    }

    public void TurnLeft()
    {
        if (!GetComponent<Fish>().isActiveFish) return;

        Debug.Log("Reel Left Input Received");
        if (direction != -1)
        {
            direction = -1;
            SetTargetPosition(transform.position);
            transform.LookAt(targetPosition);
        }
    }

    public void TurnRight()
    {
        if (!GetComponent<Fish>().isActiveFish) return;
        if (direction != 1)
        {
            direction = 1;
            SetTargetPosition(transform.position);
            transform.LookAt(targetPosition);
        }
    }

    public bool IsInLineRange()
    {
        return transform.position.x >= xLineLeftRange && transform.position.x <= xLineRightRange;
    }

    public bool IsInLeftWarningRange()
    {
        return transform.position.x >= xLineLeftWarningRange && transform.position.x < xLineLeftRange;
    }
    public bool IsInRightWarningRange()
    {
        return transform.position.x > xLineRightWarningRange && transform.position.x <= xLineRightRange;
    }


}
