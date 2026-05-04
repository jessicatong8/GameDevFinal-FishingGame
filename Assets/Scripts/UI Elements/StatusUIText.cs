using UnityEngine;
using TMPro;

public class StatusUIText : MonoBehaviour
{
    private TextMeshProUGUI statusText;
    private FishingManager fishingManager;
    // private Vector3 originalPosition;
    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHook;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay;

        FishingAreaTrigger.OnPlayerEnterFishingArea += HandleExitEnter;
        FishingAreaTrigger.OnPlayerExitFishingArea += HandleExitEnter;
        PlayerInputState.MenuTogglePerformed += HandleMenuToggle;

    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHook;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;

        FishingAreaTrigger.OnPlayerEnterFishingArea -= HandleExitEnter;
        FishingAreaTrigger.OnPlayerExitFishingArea -= HandleExitEnter;
        PlayerInputState.MenuTogglePerformed -= HandleMenuToggle;
    }
    void Start()
    {
        statusText = GetComponent<TextMeshProUGUI>();
        fishingManager = FishingManager.Instance;
        ClearText();
    }

    private void ClearText()
    {
        statusText.text = "";
    }
    private void SetGamePlayText()
    {
        statusText.text = "Press F to start fishing";
    }
    void HandleCast()
    {
        ClearText();
    }
    void HandleBite()
    {
        statusText.text = "A fish has bitten, press SPACE to reel!";
    }
    void HandleHook()
    {
        ClearText();
    }
    // HandleCaught() is now handled by CatchPresentationUI to show different text based on fish type
    // Mainly for a cleaner catch panel and repositioned text to be above the fish presentation
    void HandleMenuToggle()
    {
        if (PlayerInputState.Instance.CurrentState == PlayerInputState.InputStates.Menu)
        {
            SetGamePlayText();
        }
        else
        {
            if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Gameplay || FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.CatchPresentation)
            {
                ClearText();
            }
        }
    }
    void HandleReturnToGameplay()
    {
        string reason = fishingManager.ReturnToGameplayReason;
        switch (reason)
        {
            case "HookWindowTimedOut":
                statusText.text = "Oops your fish got away! You didn’t hook in time";
                StartCoroutine(EscapedMessageDelay(2.5f));
                // SetGamePlayText();
                break;
            case "DripLevelTooLow":
                statusText.text = "You were mogged, your face card declined and the fish swam away.";
                StartCoroutine(EscapedMessageDelay(2.5f));
                // SetGamePlayText();
                break;
            case "OutOfLineRange":
                statusText.text = "Oops your fish got away!";
                StartCoroutine(EscapedMessageDelay(1.5f));
                // SetGamePlayText();
                break;
            case "TensionOutOfRange":
                statusText.text = "Oops your fish got away!";
                StartCoroutine(EscapedMessageDelay(1.5f));
                break;
            case "PlayerAborted":
                SetGamePlayText();
                break;
            case "LevelUpConfirmed":
                SetGamePlayText();
                break;
            case "CatchConfirmed":
                SetGamePlayText();
                break;
        }
    }
    private void HandleExitEnter(bool isInFishingArea)
    {
        if (!isInFishingArea)
        {
            statusText.text = "You left the fishing area, come back to fish!";
        }
        else
        {
            SetGamePlayText();
        }
    }
    private System.Collections.IEnumerator EscapedMessageDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Gameplay)
        {
            SetGamePlayText();
        }
        else
        {
            ClearText();
        }
    }
}
