using UnityEngine;

public class FishSequenceManager : MonoBehaviour
{

    private static FishSequenceManager instance;
    public static FishSequenceManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<FishSequenceManager>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Fish[] fishSequence;
    // [SerializeField] private int fishSequenceIndex;

    private void Awake()
    {
        instance = this;
        ResetFishData();
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
    }



    public Fish GetNextFishFromSequence()
    {
        if (fishSequence == null || fishSequence.Length == 0)
        {
            DebugLogger.Instance.LogWarning("FishingManager: Fish sequence is null or empty, but usePrototypeFishSequence is true. Please assign fish to the sequence in the inspector.");
            return null;
        }

        Debug.Log("fish sequence index: " + FishData.fishSequenceIndex);

        if (FishData.fishSequenceIndex < fishSequence.Length)
        {
            Fish nextFish = fishSequence[FishData.fishSequenceIndex];
            if (nextFish != null)
            {
                return nextFish;
            }

        }

        return null;
    }

    private void HandleCaught()
    {
        Debug.Log("A fish was caught, checking for game win.");
        if (FishData.fishSequenceIndex == fishSequence.Length - 1)
        {
            Debug.Log("All fish in sequence have been caught, invoking game win and resetting fish data.");
            FishingManager.Instance.InvokeGameWin();
            ResetFishData();
            return;
        }
        FishData.fishSequenceIndex++;
    }

    public void ResetFishData()
    {
        FishData.numFishCaught = 0;
        FishData.fishSequenceIndex = 0;
    }

    public void AddFishCaught()
    {
        FishData.numFishCaught += 1;
    }

    public int GetNumFishCaught()
    {
        return FishData.numFishCaught;
    }

}