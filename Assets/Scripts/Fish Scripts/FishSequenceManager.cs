using UnityEngine;

public class FishSequenceManager : MonoBehaviour
{

    private static FishSequenceManager instance;

    [SerializeField] private Fish[] fishSequence;
    [SerializeField] private Fish[] prototypeFishSequence;

    public bool usePrototypeFishSequence;
    // [SerializeField] private int fishSequenceIndex;

    private void Awake()
    {
        instance = this;
        ResetFishData();
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        // FishingManager.OnGameWin += ResetFishData;
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        // FishingManager.OnGameWin -= ResetFishData;
    }



    public Fish GetNextFishFromSequence()
    {
        Fish[] sequence = usePrototypeFishSequence ? prototypeFishSequence : fishSequence;


        if (sequence == null || sequence.Length == 0)
        {
            DebugLogger.Instance.LogWarning("FishingManager: Fish sequence is null or empty, but usePrototypeFishSequence is true. Please assign fish to the sequence in the inspector.");
            return null;
        }

        Debug.Log("fish sequence index: " + FishData.fishSequenceIndex);

        if (FishData.fishSequenceIndex < sequence.Length)
        {
            Fish nextFish = sequence[FishData.fishSequenceIndex];
            if (nextFish != null)
            {
                return nextFish;
            }

        }

        return null;
    }

    private void HandleCaught()
    {
        IncrementFishSequenceIndex();
    }

    public bool CheckGameWin()
    {
        Fish[] sequence = usePrototypeFishSequence ? prototypeFishSequence : fishSequence;

        if (FishData.fishSequenceIndex == sequence.Length)
        {
            ResetFishData();
            return true;
        }
        return false;
    }

    public void ResetFishData()
    {
        FishData.numFishCaught = 0;
        FishData.fishSequenceIndex = 0;
    }

    public void IncrementFishSequenceIndex()
    {
        FishData.fishSequenceIndex++;
    }

    public void IncrementFishCaught()
    {
        FishData.numFishCaught += 1;
    }

    public int GetNumFishCaught()
    {
        return FishData.numFishCaught;
    }

}