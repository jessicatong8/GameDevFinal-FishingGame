using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]

public class CatchPresentationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI catchText;

    private void Awake()
    {
        if (catchText == null)
        {
            catchText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (catchText != null)
        {
            catchText.gameObject.SetActive(false);
        }
    }

    private void OnValidate()
    {
        if (catchText == null)
        {
            catchText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        // FishingManager.OnCatchConfirmationEnd += HandleReturnToIdle;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay; // when player confirms catch or abort/escape 
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        // FishingManager.OnCatchConfirmationEnd -= HandleReturnToIdle;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }

    private void HandleCaught()
    {
        if (catchText == null)
        {
            DebugLogger.Instance.LogWarning("CatchPresentationUI: catchText is not assigned. Add a TextMeshProUGUI child or assign the field in the inspector.");
            return;
        }

        Fish activeFish = FishingManager.Instance != null ? FishingManager.Instance.activeFish : null;
        string fishName = activeFish != null ? activeFish.fishName : "fish";

        catchText.text = $"You caught a {fishName}!";
        catchText.gameObject.SetActive(true);
    }

    private void HandleReturnToGameplay()
    {
        HideCatchText();
    }

    private void HideCatchText()
    {
        if (catchText == null) return; 
        catchText.gameObject.SetActive(false);
    }
}
