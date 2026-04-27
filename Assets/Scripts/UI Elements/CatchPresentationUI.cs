using TMPro;
using UnityEngine;

public class CatchPresentationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI catchText;

    private void Awake()
    {
        if (catchText != null)
        {
            catchText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        FishingManager.OnCaught += HandleCaught;
        // FishingManager.OnCatchConfirmationEnd += HandleReturnToIdle;
        FishingManager.OnReturnToIdle += HandleReturnToIdle; // when player confirms catch or abort/escape 
    }

    private void OnDisable()
    {
        FishingManager.OnCaught -= HandleCaught;
        // FishingManager.OnCatchConfirmationEnd -= HandleReturnToIdle;
        FishingManager.OnReturnToIdle -= HandleReturnToIdle;
    }

    private void HandleCaught()
    {
        if (catchText == null)
        {
            return;
        }

        Fish activeFish = FishingManager.Instance != null ? FishingManager.Instance.activeFish : null;
        string fishName = activeFish != null ? activeFish.fishName : "fish";

        catchText.text = $"You caught a {fishName}!";
        catchText.gameObject.SetActive(true);
    }

    private void HandleReturnToIdle()
    {
        HideCatchText();
    }

    private void HideCatchText()
    {
        if (catchText == null)
        {
            return;
        }

        catchText.gameObject.SetActive(false);
    }
}
