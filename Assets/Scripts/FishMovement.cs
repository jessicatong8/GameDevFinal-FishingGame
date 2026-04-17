using System;
using System.Collections;
using UnityEngine;

public class FishMovement : MonoBehaviour
{

    public static event Action LeavingLineRange;
    public static event Action EnteringLineRange;

    private Animator animator;
    public FishingManager fishManager;
    private Vector3 position;
    private Vector3 targetPosition;
    private float swimmingSpeed;

    // will likely have to adjust these based on the camera view 
    public float xLeftBoundary = -15f;
    public float xRightBoundary = 15f;
    public float xLineLeftRange = -5f;
    public float xLineRightRange = 5f;

    public float arrivalThreshold = 0.1f;


    private int direction = 1; // 1 for right, -1 for left




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

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
        Debug.Log("Initial Position: " + position);

    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Current Position: " + transform.position);
        SwimTowardTarget();
        CheckInLineRange();

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
            // Debug.Log("Arrived at Target Position: " + targetPosition);
            transform.position = targetPosition;
            SetTargetPosition(transform.position);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

    }

    private void CheckInLineRange()
    {
        if (transform.position.x < xLineLeftRange || transform.position.x > xLineRightRange)
        {
            LeavingLineRange?.Invoke();
        }
        else
        {
            EnteringLineRange?.Invoke();
        }
    }

}
