using UnityEngine;
using System.Collections.Generic;

public class ManualHatPicker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The parent object containing all hat models.")]
    public Transform hatContainer; 
    
    [SerializeField] private int currentHatIndex = 0;
    private List<GameObject> hats = new List<GameObject>();

    // This allows you to right-click the component in the Inspector to trigger it
    [ContextMenu("Cycle Hat")]
    public void CycleHat()
    {
        if (hats.Count == 0) RefreshHatList();
        
        currentHatIndex = (currentHatIndex + 1) % hats.Count;
        UpdateHatVisibility();
    }

    private void OnValidate()
    {
        // Runs whenever you change values in the Inspector
        if (hatContainer != null)
        {
            RefreshHatList();
            UpdateHatVisibility();
        }
    }

    private void RefreshHatList()
    {
        hats.Clear();
        // Mimicking the source code's way of finding children under customize_objects
        foreach (Transform child in hatContainer)
        {
            hats.Add(child.gameObject);
        }
    }

    private void UpdateHatVisibility()
    {
        if (hats.Count == 0) return;

        // Ensure index stays within bounds
        currentHatIndex = Mathf.Clamp(currentHatIndex, 0, hats.Count - 1);

        // Toggle logic based on the ApplySelections method
        for (int i = 0; i < hats.Count; i++)
        {
            if (hats[i] != null)
            {
                hats[i].SetActive(i == currentHatIndex);
            }
        }
    }
}