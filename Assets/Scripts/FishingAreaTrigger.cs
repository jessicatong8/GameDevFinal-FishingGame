using UnityEngine;
using System;
using System.Collections.Generic;

public class FishingAreaTrigger : MonoBehaviour
{
    public static bool IsPlayerInFishingArea { get; private set; }
    public static event Action<bool> OnPlayerEnterFishingArea;
    public static event Action<bool> OnPlayerExitFishingArea;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player entered fishing area.");
        IsPlayerInFishingArea = true;
        OnPlayerEnterFishingArea?.Invoke(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player exited fishing area.");
        IsPlayerInFishingArea = false;
        OnPlayerExitFishingArea?.Invoke(false);
    }

    private void OnDisable()
    {
        IsPlayerInFishingArea = false;
    }
}
