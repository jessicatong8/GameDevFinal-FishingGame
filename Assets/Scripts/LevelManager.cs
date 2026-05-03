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
    // private bool playerLeveledUpThisCatch = false; // tracks if player leveled up on this catch before confirming level up presentation
    void Awake()
    {
        ResetPlayerLevel();
    }
    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        PlayerInputState.CatchConfirmPerformed += HandleCatchConfirmed;
        PlayerInputState.LevelConfirmPerformed += HandleLevelConfirmed;
    }
    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        PlayerInputState.CatchConfirmPerformed -= HandleCatchConfirmed;
        PlayerInputState.LevelConfirmPerformed -= HandleLevelConfirmed;

    }
    public int GetPlayerLevel()
    {
        return PlayerData.playerLevel;
    }
    private void IncrementPlayerLevel()
    {
        PlayerData.playerLevel++;
        // playerLeveledUpThisCatch = true;
        DebugLogger.Instance.LogMethodCall($"Player leveled up to {PlayerData.playerLevel}!");
    }
    private void ResetPlayerLevel()
    {
        PlayerData.playerLevel = 1;
    }
    private void HandleCaught()
    {
        // playerLeveledUpThisCatch = false; // reset flag at start of catch
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
            FishingManager.Instance.TransitionToLevelUpPresentation();
        }

        else if (FishSequenceManager.Instance.CheckGameWin()) //TODO move checkGameWin here
        {
            HandleGameWin();
        }
        else
        {
            FishingManager.Instance.ReturnToGameplay("CatchConfirmed");
        }

    }

    private void HandleLevelConfirmed()
    {
        FishingManager.Instance.ReturnToGameplay("LevelUpConfirmed");
    }


    private void HandleGameWin()
    {
        Debug.Log("All fish in sequence have been caught, invoking game win.");
        FishingManager.Instance.InvokeGameWin();
        SceneManager.LoadScene("WinScene");
    }
}