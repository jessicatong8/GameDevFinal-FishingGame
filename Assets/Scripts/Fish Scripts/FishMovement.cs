using System;
using UnityEngine;

public class FishMovement : MonoBehaviour
{

    // [SerializeField] private LineRangeManager lineRangeManager;

    // public static event Action LeavingLineRange;
    // public static event Action EnteringLineRange;

    private Vector3 position;
    private Vector3 targetPosition;
    private float swimmingSpeed;

    // will likely have to adjust these based on the camera view 
    private float xLeftBoundary = -15f;
    private float xRightBoundary = 15f;

    private float xLineLeftWarningRange = -4f;
    private float xLineRightWarningRange = 4f;
    private float xLineLeftRange = -5f;
    private float xLineRightRange = 5f;

    public float arrivalThreshold = 0.1f;
    private int direction = 1; // 1 for right, -1 for left
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // xLineLeftWarningRange = lineRangeManager.xLineLeftWarningRange;
        // xLineRightWarningRange = lineRangeManager.xLineRightWarningRange;
        // xLineLeftRange = lineRangeManager.xLineLeftRange;
        // xLineRightRange = lineRangeManager.xLineRightRange;


        position = transform.position;
        swimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        SetTargetPosition(position);
        // DebugLogger.Instance.Log("Initial Position: " + position);

    }

    private void OnEnable()
    {

        PlayerInputState.Instance.ReelLeftPerformed += TurnLeft;
        PlayerInputState.Instance.ReelRightPerformed += TurnRight;
    }

    private void OnDisable()
    {
        PlayerInputState.Instance.ReelLeftPerformed -= TurnLeft;
        PlayerInputState.Instance.ReelRightPerformed -= TurnRight;
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
        // Debug.Log("Reel Left Input Received");
        if (!GetComponent<Fish>().isActiveFish)
        {
            // Debug.Log("No active fish, ignoring input.");
            return;
        }


        if (direction != -1)
        {
            direction = -1;
            SetTargetPosition(transform.position);
            transform.LookAt(targetPosition);
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

        return transform.position.x <= xLineLeftWarningRange && transform.position.x > xLineLeftRange;
    }
    public bool IsInRightWarningRange()
    {
        return transform.position.x > xLineRightWarningRange && transform.position.x <= xLineRightRange;
    }
}
