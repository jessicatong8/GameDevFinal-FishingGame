using UnityEngine;

public class FishSpawner : MonoBehaviour 
{
    public Fish[] possibleFish;

    public Fish GetRandomFish() 
    {
        int totalWeight = 0;

        for(int i = 0; i < possibleFish.Length ; i++)
        {
            Fish fish = possibleFish[i];
            totalWeight += fish.rarityWeight;
        }

        int randomFish = UnityEngine.Random.Range(0, totalWeight);
        
        int currentWeight = 0;

        for(int i = 0; i < possibleFish.Length ; i++)
        {
            Fish fish = possibleFish[i];
            currentWeight += fish.rarityWeight;
            if (randomFish < currentWeight)
            {
                return fish;
            }
        }

        return possibleFish[possibleFish.Length - 1]; 
    }
}

