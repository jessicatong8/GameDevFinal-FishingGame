using UnityEngine;

public class PlayerLevelManager : MonoBehaviour
{
    private static PlayerLevelManager instance;
    public static PlayerLevelManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<PlayerLevelManager>();
            return instance;
        }
        private set => instance = value;
    }
    void Awake()
    {
        ResetPlayerLevel();
    }
    public int GetPlayerLevel()
    {
        return PlayerData.playerLevel;
    }

    public void IncrementPlayerLevel()
    {
        PlayerData.playerLevel++;
    }

    public void ResetPlayerLevel()
    {
        PlayerData.playerLevel = 1;
    }

}