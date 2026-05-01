using UnityEngine;
using System.Collections.Generic;

public class ManualHatPicker : MonoBehaviour
{
    [SerializeField] private List<GameObject> hats = new List<GameObject>(); // will have 3 hats
    [SerializeField] private int currentHatIndex = 0;
    public void Start()
    {
        ShowOnlyThisHat(0);
    }
    public void OnEnable()
    {
        PlayerInputState.CycleHatPerformed += CycleHat;
    }
    public void OnDisable()
    {
        PlayerInputState.CycleHatPerformed -= CycleHat;
    }
    public void CycleHat()
    {
        currentHatIndex = (currentHatIndex + 1) % (hats.Count+1); // allows for values: 0, 1, 2, 3, then back to 0
        ShowOnlyThisHat(currentHatIndex);
    }
    private void OnValidate()
    // runs whenever you change inspector values 
    {
        ShowOnlyThisHat(currentHatIndex);
    }
    private void ShowOnlyThisHat(int hatIndex)
    {
        if (currentHatIndex == 0)    // hatless
        {
            for (int i = 0; i < hats.Count; i++)
            {
                hats[i].SetActive(false);
            }
        }
        else                    // hats 1,2,3
        {
            for (int i = 1; i <= hats.Count; i++) // 
            {
                // Debug.Log($"ManualHatPicker.ShowOnlyThisHat: Setting hat {i} active: {i == hatIndex + 1}");
                // hats[] has 0,1,2 but maps to hatIndex 1,2,3. both i and hatIndex are 1-based to skip the hatless option at index 0.
                hats[i-1].SetActive(i == hatIndex);
            }
        }
    }
}