using UnityEngine;

public class FishManager : MonoBehaviour
{

    private static FishManager instance;
    public static FishManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindFirstObjectByType<FishManager>();
            return instance;
        }
        private set => instance = value;
    }

    [SerializeField] private Fish[] fishSequence;
    [SerializeField] private int fishSequenceIndex;

    private void Awake()
    {
        instance = this;
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


        if (fishSequenceIndex < fishSequence.Length)
        {
            Fish nextFish = fishSequence[fishSequenceIndex];
            if (nextFish != null)
            {
                return nextFish;
            }

        }
        return null;
    }

    private void HandleCaught()
    {
        fishSequenceIndex++;
    }

    public void ResetFishData()
    {
        FishData.numFishCaught = 0;
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