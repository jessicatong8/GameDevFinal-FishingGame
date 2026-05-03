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
        Debug.Log("You've caught " + numFishCaught + " fish!");
        if (numFishCaught == 2 || numFishCaught == 5 || numFishCaught == 9 || numFishCaught == 10) // according to our preset fish sequence
        {
            IncrementPlayerLevel();
        }
    }
}