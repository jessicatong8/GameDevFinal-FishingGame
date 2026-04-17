using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class FishingManager : MonoBehaviour
{
    //make events for future ui and audio 
    public static event Action OnCast;
    public static event Action OnBite;
    public static event Action OnReel;
    public static event Action OnReelingActive;
    public static event Action OnReelingInactive;
    public static event Action OnWiggle; //should this be an event?
    public static event Action OffWiggle; //should this be an event too

    public static event Action OnCaught;
    public static event Action OnLineBreak;
    public static event Action OnReturnToIdle;

    //maybe change getters at bottom to events like this 
    public static event Action<float> OnProgressUpdated;
    public static event Action<float> OnTensionUpdated;

    public FishSpawner spawner;
    private Fish activeFish;
    public float hookTimer;
    // 
    public enum FishingState
    {
        Idle,
        Waiting,
        HookWindow,
        Reeling
    }

    private FishingState currentState = FishingState.Idle;
    private float progress;
    private float tension;
    private float timer;
    private bool isWiggling;

    void Update()
    {
        //condition to escape any time during fishing
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            //do we need an event here to trigger idle?
            AbortFishing();

        }
        switch (currentState)
        {
            case FishingState.Idle: //switch from idle to casting
                if (Keyboard.current.spaceKey.wasPressedThisFrame) //this or other input system???
                {
                    StartWaiting(); //next round it will go to handle waiting
                }
                break;

            case FishingState.Waiting:
                HandleWaiting();
                break;

            case FishingState.HookWindow:
                HandleHookWindow();
                break;

            case FishingState.Reeling:
                HandleReeling();
                break;
        }
    }

    //
    void StartWaiting()
    {
        OnCast?.Invoke();
        activeFish = spawner.GetRandomFish();
        activeFish.isActive = true;
        Debug.Log(activeFish.fishName);
        //call event to trigger casting ui and sound 
        //does this go here or in update
        //do i want seperate event for after casting for an idle bobbing wait

        currentState = FishingState.Waiting; //change state
        timer = UnityEngine.Random.Range(2f, 5f); //random time to wait for bite
    }

    void HandleWaiting()
    {
        timer -= Time.deltaTime; // count down the clock
        if (timer <= 0) //once we get a bite
        {
            OnBite?.Invoke(); //do i again put this right here

            currentState = FishingState.HookWindow; //change state

            timer = hookTimer; //sets timer to hook window time
        }
    }

    void HandleHookWindow()
    {
        timer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnReel?.Invoke(); //change ui to reflecting reeling state
            StartReeling();
        }
        else if (timer <= 0)
        {
            StartWaiting(); //fish got away but it was during initial part of fishing so 
            // we make it go back to jsut after casting the rod
        }
    }

    void StartReeling()
    {
        currentState = FishingState.Reeling; //change state
        progress = 0;
        tension = 0;
        timer = activeFish.wiggleOffTimer; // reuse timer for wiggling
    }

    void HandleReeling()
    {
        HandleWiggleTimer();

        if (Keyboard.current.spaceKey.isPressed)
        {
            OnReelingActive?.Invoke();
            if (isWiggling)
            {
                tension += activeFish.wiggleStrength * Time.deltaTime;
                OnTensionUpdated?.Invoke(tension);
            }
            else
            {
                progress += activeFish.reelingSpeed * Time.deltaTime;
                OnProgressUpdated?.Invoke(progress);
            }
        }
        else
        {
            OnReelingInactive?.Invoke();
            // tension drops when you aren't pulling
            tension -= activeFish.tensionDropRate * Time.deltaTime;
            tension = Mathf.Max(tension, 0); //we dont want our tension to drop below 0 

            OnTensionUpdated?.Invoke(tension);
        }


        if (progress >= 100)
        {
            OnCaught?.Invoke();
            AbortFishing();
            //go back to idle
        }
        if (tension >= activeFish.maxTension)
        {
            OnLineBreak?.Invoke();
            AbortFishing();
            //go back to idle
        }


    }

    void HandleWiggleTimer()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isWiggling = !isWiggling; //flip wiggling state to new one
            //now if it is supposed to start wiggling
            if (isWiggling == true)
            {
                OnWiggle?.Invoke();
                timer = activeFish.wiggleOnTimer; //maybe set based on fish
            }
            else
            {
                OffWiggle?.Invoke();
                timer = activeFish.wiggleOffTimer; //is this done right
            }
        }
    }

    void AbortFishing()
    {
        currentState = FishingState.Idle;
        activeFish.isActive = false;
        activeFish = null;
        OnReturnToIdle?.Invoke();
    }

}