using System;
using System.Collections;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    // will likely have to adjust these based on the camera view 
    public float xLeftBoundary = -15f;
    public float xRightBoundary = 15f;
    public float outerLineRange = 8f;
    public float innerLineRange = 5f;
    public float arrivalThreshold = 0.1f;
    public float idleSwimmingSpeed = 2f;

    private Fish fish;
    private float reelingSwimmingSpeed;
    private int direction = 1; // 1 for right, -1 for left
    private Vector3 position;
    private Vector3 targetPosition;

    private FishingManager fishManager;
    private enum FishingState
    {
        Idle,
        Casting,
        Reeling,
        Caught,
        Escaped
    }
    private FishingState currentState;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fish = GetComponent<Fish>();
        currentState = FishingState.Idle;
        position = transform.position;
        reelingSwimmingSpeed = GetComponent<Fish>().swimmingSpeed;

        FishingManager.OnBite += OnBite;
        FishingManager.OnHooked += OnHooked;
        FishingManager.OnCaught += OnCaught;
        FishingManager.OnEscaped += OnEscaped;

        SetTargetPosition(position);
        Debug.Log("Initial Position: " + position);

    }

    private void OnBite() => currentState = FishingState.Casting;
    private void OnHooked() => currentState = FishingState.Reeling;
    private void OnCaught() => currentState = FishingState.Caught;
    private void OnEscaped() => currentState = FishingState.Escaped;

    void OnDestroy()
    {
        FishingManager.OnBite -= OnBite;
        FishingManager.OnHooked -= OnHooked;
        FishingManager.OnCaught -= OnCaught;
        FishingManager.OnEscaped -= OnEscaped;
    }

    void Update()
    {

        switch (currentState)
        {
            case FishingState.Idle: HandleIdleMovement(); break;
            case FishingState.Casting: HandleCastingMovement(); break;
            case FishingState.Reeling: HandleReelingMovement(); break;
            case FishingState.Caught: HandleCaughtMovement(); break;
            case FishingState.Escaped: HandleEscapedMovement(); break;
        }

    }

    private void HandleIdleMovement()
    {
        Debug.Log("Idle Movement");
        SwimTowardTarget(idleSwimmingSpeed);
    }

    private void HandleCastingMovement()
    {
        Debug.Log("Casting Movement");
        if (fish.isActive)
        {
            transform.position = new Vector3(0, transform.position.y, transform.position.z); // Move the fish to the center of the screen
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180, transform.eulerAngles.z); // Face the fish forward
        }
    }
    private void HandleReelingMovement()
    {
        Debug.Log("Reeling Movement");
        SwimTowardTarget(reelingSwimmingSpeed);
        // TODO: functions to check for range
    }

    private void HandleCaughtMovement()
    {
        Debug.Log("Caught Movement");
        transform.position = new Vector3(0, transform.position.y, transform.position.z); // Move the fish to the center of the screen
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180, transform.eulerAngles.z); // Face the fish forward
        // TODO - make it fly out of the water or add some sort of celebration animation or effect here
        IEnumerator CaughtAnimation()
        {
            // Placeholder for caught animation logic
            yield return new WaitForSeconds(2f); // Simulate animation duration
            // TODO: destroy the fish? and unsubscribe from events? or maybe just set it to inactive and move it back to the pool?

        }
        StartCoroutine(CaughtAnimation());

    }

    private void HandleEscapedMovement()
    {
        Debug.Log("Escaped Movement");
        SetTargetPosition(transform.position); // Set a new target position to swim towards
        currentState = FishingState.Idle; // Switch back to idle state to swim towards the new target position
    }

    public Vector3 SetTargetPosition(Vector3 position)
    {
        if (position.x < 0)
        {
            direction = 1; // Move right
            targetPosition = new Vector3(UnityEngine.Random.Range(outerLineRange, xRightBoundary), position.y, position.z);
        }
        else if (position.x > 0)
        {
            direction = -1; // Move left
            targetPosition = new Vector3(UnityEngine.Random.Range(xLeftBoundary, -outerLineRange), position.y, position.z);
        }
        Debug.Log("Target Position: " + targetPosition);
        return targetPosition;
    }

    private void SwimTowardTarget(float swimmingSpeed)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            Debug.Log("Arrived at Target Position: " + targetPosition);
            transform.position = targetPosition;
            SetTargetPosition(transform.position);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, swimmingSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, direction * 90f, transform.eulerAngles.z); // Flip the fish to face the right direction

        // TODO - add some sort of random vertical movement to make it less linear and more natural
        // TODO - ease in and out tweening for more natural movement
    }

    public bool CheckInLineRange()
    {
        if (transform.position.x < -outerLineRange || transform.position.x > outerLineRange)
        {
            Debug.Log("Fish is out of line range!");
            return false;
        }
        else
        {
            return true;
        }
    }

}
