using UnityEngine;
using System.Collections.Generic;

public class Hats : MonoBehaviour
{
    [SerializeField] private List<GameObject> hats = new List<GameObject>(); // will have 3 hats
    [SerializeField] private int numHats = 0;
    public void Start()
    {
        ShowHats(0);
    }
    public void OnEnable()
    {
        FishingManager.OnLevelUp += HandleLevelUp;
        PlayerInputState.CycleHatPerformed += CycleHat;
    }
    public void OnDisable()
    {
        FishingManager.OnLevelUp -= HandleLevelUp;
        PlayerInputState.CycleHatPerformed -= CycleHat;
    }
    public void CycleHat()
    {
        numHats = (numHats + 1) % (hats.Count + 1); // allows for values: 0, 1, 2, 3, then back to 0
        ShowHats(numHats);
    }
    private void OnValidate()
    // runs whenever you change inspector values 
    {
        ShowHats(numHats);
    }
    private void ShowHats(int numHats)
    // Shows hats stacked on top of each other given how many hats should be displayed
    {
        if (numHats == 0)    // hatless
        {
            for (int i = 0; i < hats.Count; i++)
            {
                hats[i].SetActive(false);
            }
        }
        else                    // hats 1,2,3
        {
            for (int i = 0; i <= numHats - 1; i++) // 
            {
                // Debug.Log($"ManualHatPicker.ShowOnlyThisHat: Setting hat {i} active: {i == hatIndex + 1}");
                // hats[] has 0,1,2 but maps to hatIndex 1,2,3. both i and hatIndex are 1-based to skip the hatless option at index 0.
                hats[i].SetActive(true);
            }
        }
    }

    private void HandleLevelUp()
    {
        ShowHats(LevelManager.Instance.GetPlayerLevel() - 1); // playerLevel is 1-based and numHats is 0-based, so subtract 1 to get correct index
    }
}