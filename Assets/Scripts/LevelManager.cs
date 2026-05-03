using UnityEngine;
using System;

using UnityEngine.SceneManagement;

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

    public static event Action<int> OnLevelUp; // after a catch is confirmed, when player levels up by catching a certain number of fish
    public static event Action OnGameWin; // after a catch is confirmed, when player catches all fish and wins the game

    void Awake()
    {
        ResetPlayerLevel();
    }
    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        PlayerInputState.ConfirmCatchPerformed += HandleCatchConfirmed;
    }
    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
    }
    public int GetPlayerLevel()
    {
        return PlayerData.playerLevel;
    }
    private void IncrementPlayerLevel()
    {
        PlayerData.playerLevel++;
        DebugLogger.Instance.LogMethodCall($"Player leveled up to {PlayerData.playerLevel}!");
        OnLevelUp?.Invoke(PlayerData.playerLevel);
    }
    private void ResetPlayerLevel()
    {
        PlayerData.playerLevel = 1;
    }
    private void HandleCaught()
    {
        FishSequenceManager.Instance.IncrementFishCaught();
    }

    private void HandleCatchConfirmed()
    {
        // Invoke events for leveling player up and winning the game

        int numFishCaught = FishSequenceManager.Instance.GetNumFishCaught();
        Debug.Log("You've caught " + numFishCaught + " fish!");
        if (numFishCaught == 2 || numFishCaught == 5 || numFishCaught == 9 || numFishCaught == 10) // according to our preset fish sequence
        {
            IncrementPlayerLevel();
        }
        if (FishSequenceManager.Instance.CheckGameWin()) //TODO move checkGameWin here
        {
            HandleGameWin();
        }
    }

    private void HandleGameWin()
    {
        Debug.Log("All fish in sequence have been caught, invoking game win.");
        OnGameWin?.Invoke();
        SceneManager.LoadScene("WinScene");
    }
}