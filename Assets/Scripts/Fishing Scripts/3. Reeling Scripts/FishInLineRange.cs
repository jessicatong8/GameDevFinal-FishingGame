using System.Collections;
using UnityEngine;

public class FishInLineRange : MonoBehaviour
{
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
        position = transform.position;
        swimmingSpeed = GetComponent<Fish>().swimmingSpeed;
        SetTargetPosition(position);
        DebugLogger.Instance.Log("Initial Position: " + position);

    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Current Position: " + transform.position);
        SwimTowardTarget();

    }

    public Vector3 SetTargetPosition(Vector3 position)
    {
        if (position.x <= 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(Random.Range(xLineRightRange, xRightBoundary), position.y, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(Random.Range(xLeftBoundary, xLineLeftRange), position.y, position.z);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        DebugLogger.Instance.Log("Target Position: " + targetPosition);
        return targetPosition;
    }

    private void SwimTowardTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            DebugLogger.Instance.Log("Arrived at Target Position: " + targetPosition);
            transform.position = targetPosition;
            SetTargetPosition(transform.position);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);

    }

}
