using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class StatusUI : MonoBehaviour
{

    private TextMeshProUGUI statusText;

    void OnEnable()
    {
        FishingManager.OnCast += HandleCast;
        FishingManager.OnBite += HandleBite;
        FishingManager.OnHook += HandleHook;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay;
    }

    void OnDisable()
    {
        FishingManager.OnCast -= HandleCast;
        FishingManager.OnBite -= HandleBite;
        FishingManager.OnHook -= HandleHook;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }

    void Start()
    {
        statusText = GetComponent<TextMeshProUGUI>();
        ClearText();
    }
    private void ClearText()
    {
        statusText.text = "";

    }

    void HandleCast()
    {
        ClearText();
    }

    private void SetGamePlayText()
    {
        statusText.text = "Press F to start fishing";
    }
    void HandleBite()
    {
        statusText.text = "A fish has bitten, press space to reel!";
    }
    void HandleHook()
    {
        ClearText();
    }

    void HandleReturnToGameplay()
    {
        string reason = FishingManager.Instance.escapeReason;
        switch (reason)
        {
            case "HookWindowTimedOut":
                statusText.text = "Oops your fish got away! You didn’t hook in time";
                StartCoroutine(EscapedMessageDelay(2.5f));
                // SetGamePlayText();
                break;
            case "DripLevelTooLow":
                statusText.text = "The fish ignored you because you aren’t drippy enough yet";
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
            case "FishCaughtConfirmed":
                SetGamePlayText();
                break;
        }
        ;
    }

    private System.Collections.IEnumerator EscapedMessageDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // ClearText();
        SetGamePlayText();
    }



}
