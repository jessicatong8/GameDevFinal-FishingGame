using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<LevelManager>();
            return instance;
        }
        private set => instance = value;
    }
    void Awake()
    {
        ResetPlayerLevel();
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnGameWin += ResetPlayerLevel;
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnGameWin -= ResetPlayerLevel;
    }
    public int GetPlayerLevel()
    {
        return PlayerData.playerLevel;
    }

    public void IncrementPlayerLevel()
    {
        PlayerData.playerLevel++;
        FishingManager.Instance.InvokeLevelUp(PlayerData.playerLevel);
    }

    public void ResetPlayerLevel()
    {
        PlayerData.playerLevel = 1;
    }

    private void HandleCaught()
    {
        FishSequenceManager.Instance.IncrementFishCaught();
        int numFishCaught = FishSequenceManager.Instance.GetNumFishCaught();

        // Debug.Log("Fish was caught, checking if you can level up. Current level: " + GetPlayerLevel());

        Debug.Log("You've caught " + numFishCaught + " fish!");
        if (numFishCaught == 2 || numFishCaught == 5 || numFishCaught == 9 || numFishCaught == 10) // according to our preset fish sequence
        {
            IncrementPlayerLevel();
            // Debug.Log("Enough fish were caught, you have leveled up. Current level: " + GetPlayerLevel());

        }
        // CheckGameWin();
    }

    // public void CheckGameWin()
    // {
    //     Debug.Log("A fish was caught, checking for game win.");


    //     if (GetPlayerLevel() == 5)
    //     {
    //         Debug.Log("All fish in have been caught, invoking game win!");
    //         FishingManager.Instance.InvokeGameWin();
    //         ResetPlayerLevel();

    //     }

    // }

}