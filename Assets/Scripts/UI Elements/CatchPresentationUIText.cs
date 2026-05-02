using TMPro;
using UnityEngine;
[RequireComponent(typeof(TextMeshProUGUI))]
public class CatchPresentationUI : MonoBehaviour
{
    private TextMeshProUGUI catchText;
    private void Awake()
    {
        catchText = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay; // when player confirms catch or abort/escape 
    }
    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }
    private void HandleCaught()
    {
        gameObject.SetActive(true);
        Fish caughtFish = FishingManager.Instance.activeFish;
        if (caughtFish == null)
        {
            DebugLogger.Instance.LogWarning("StatusUIText.HandleCaught called but activeFish is null.");
            return;
        }
        switch (caughtFish.level)
        {
            case 1:
                catchText.text = $"You caught an NPC fish, that’s nice i guess...";
                DebugLogger.Instance.LogWarning($"Caught a {caughtFish.fishName}");
                break;
            case 2:
                catchText.text = $"Yo that’s a valid fish, that lowkey goes hard.";
                DebugLogger.Instance.LogWarning($"Caught a {caughtFish.fishName}");
                break;
            case 3:
                catchText.text = $"Damn that fish is built different! You ate and left no crumbs.";
                break;
            case 4:
                catchText.text = $"Bro... god tier pull.";
                break;
        }
    }
    private void HandleReturnToGameplay()
    {
        gameObject.SetActive(false);
    }
}
