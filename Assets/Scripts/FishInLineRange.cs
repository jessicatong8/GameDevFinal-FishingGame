using System.Collections;
using UnityEngine;

public class FishInLineRange : MonoBehaviour
{
    private float outerLineRange;
    private float innerLineRange;

    public enum LineState
    {
        InRange,
        ExitingLeftRange,
        ExitingRightRange,
        OutOfLeftRange,
        OutOfRightRange
    }
    public LineState currentLineState;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FishMovement fishMovement = GetComponent<FishMovement>();
        outerLineRange = fishMovement.outerLineRange;
        innerLineRange = fishMovement.innerLineRange;
        currentLineState = LineState.InRange;
    }

    void Update()
    {
        if (transform.position.x < -outerLineRange)
        {
            currentLineState = LineState.OutOfLeftRange;
        }
        else if (transform.position.x > outerLineRange)
        {
            currentLineState = LineState.OutOfRightRange;
        }
        else if (transform.position.x < -innerLineRange)
        {
            currentLineState = LineState.ExitingLeftRange;
        }
        else if (transform.position.x > innerLineRange)
        {
            currentLineState = LineState.ExitingRightRange;
        }
        else
        {
            currentLineState = LineState.InRange;
        }
    }

    public LineState CheckLineState()
    {
        return currentLineState;
    }

}
