using UnityEngine;
using System;

public class DockAreaTrigger : MonoBehaviour
{
    public static event Action OnPlayerEnterDock;
    public static event Action OnPlayerExitDock;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterDock?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitDock?.Invoke();
        }
    }
}
