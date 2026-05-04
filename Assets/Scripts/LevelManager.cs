using UnityEngine;

using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private FishSequenceManager fishSequenceManager;

    void Awake()
    {
        ResetPlayerLevel();
    }
    void Start()
    {
        fishSequenceManager = FindFirstObjectByType<FishSequenceManager>();
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
        DebugLogger.Instance.LogMethodCall($"Player leveled up to {PlayerData.playerLevel}!");
    }
    private void ResetPlayerLevel()
    {
        PlayerData.playerLevel = 1;
    }
    private void HandleCaught()
    {
        fishSequenceManager.IncrementFishCaught();
    }

    private void HandleCatchConfirmed()
    {
        // Invoke events for leveling player up and winning the game

        int numFishCaught = fishSequenceManager.GetNumFishCaught();
        Debug.Log("You've caught " + numFishCaught + " fish!");
        if (numFishCaught == 2 || numFishCaught == 5 || numFishCaught == 9) // according to our preset fish sequence
        {
            IncrementPlayerLevel();
            FishingManager.Instance.TransitionToLevelUpPresentation();
        }

        else if (fishSequenceManager.CheckGameWin()) //TODO move checkGameWin here
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