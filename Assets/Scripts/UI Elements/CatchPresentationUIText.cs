using TMPro;
using UnityEngine;
public class CatchPresentationUI : MonoBehaviour
{
    [SerializeField] private GameObject catchPanel;
    private TextMeshProUGUI catchText;
    private void Awake()
    {
        if (catchPanel == null)
        {
            DebugLogger.Instance.LogError("CatchPresentationUI: CatchPanel reference is not set in the inspector.");
        }
        catchText = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay; // when player confirms catch or abort/escape 
    }
    private void Start()
    {
        catchPanel.SetActive(false);
    }
    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }
    private void HandleCaught()
    {

        catchPanel.SetActive(true);
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
                break;
            case 2:
                catchText.text = $"Yo that’s a valid fish, that lowkey goes hard.";
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
        catchPanel.SetActive(false);
    }
}
