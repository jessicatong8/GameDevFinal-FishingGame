using UnityEngine;
using System.Collections;
using TMPro;

public class FishingTutorialManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float fadeSpeed = 1f;
    
    [Header("Tutorial Texts")]
    [SerializeField] private string introTxt = "Let's start fishing! Press [E] to cast your line!";
    [SerializeField] private string waitTxt = "Now we wait for a fish to come along...";
    [SerializeField] private string biteTxt = "You've got a fish on the line! Press [SPACE] to hook it!";
    [SerializeField] private string reelTxt = "Keep the fish in the green! Press [SPACE] to reel.";

    private bool tutorialCompleted = false; //prevents race conditions
    private Coroutine tutorialRoutine; //prevents multiple conflicting coroutines from firing at the same time 

    private void Start()
    {
        canvasGroup.alpha = 0;
    }

    private void OnEnable()
    {
        FishingManager.OnCast += OnCastPerformed;
        FishingManager.OnBite += OnFishBite;
        FishingManager.OnHook += OnHookSuccess;
        FishingManager.OnCaught += EndTutorialPermanently;
        FishingAreaTrigger.OnPlayerEnterFishingArea += HandleAreaEntrance;
        FishingAreaTrigger.OnPlayerExitFishingArea += HandleAreaExit;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= OnCastPerformed;
        FishingManager.OnBite -= OnFishBite;
        FishingManager.OnHook -= OnHookSuccess;
        FishingManager.OnCaught -= EndTutorialPermanently;
        FishingAreaTrigger.OnPlayerEnterFishingArea -= HandleAreaEntrance;
        FishingAreaTrigger.OnPlayerExitFishingArea -= HandleAreaExit;
    }

    private void HandleAreaEntrance(bool isInArea)
    {
        if (!tutorialCompleted) UpdateTutorialStep(introTxt);
    }

    //for if player doesnt start fishing but walks in and out of exit area
    private void HandleAreaExit(bool isInArea)
    {
        if (!tutorialCompleted) HideTutorial();
    }

    private void OnCastPerformed() => UpdateTutorialStep(waitTxt);
    private void OnFishBite() => UpdateTutorialStep(biteTxt);
    private void OnHookSuccess() => UpdateTutorialStep(reelTxt);

    private void EndTutorialPermanently()
    {
        tutorialCompleted = true;
        HideTutorial();
        //allows us to fade out the sign before killing the script
        Invoke(nameof(DisableScript), 2f); 
    }

    private void DisableScript() => this.enabled = false;

    private void UpdateTutorialStep(string newMessage)
    {
        if (tutorialCompleted) return;
        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);

        tutorialRoutine = StartCoroutine(TransitionRoutine(newMessage));
    }

    private void HideTutorial()
    {
        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);

        tutorialRoutine = StartCoroutine(FadeUI(0f));
    }

    private IEnumerator TransitionRoutine(string newMessage)
    {
        if (canvasGroup.alpha > 0)
        {
            yield return StartCoroutine(FadeUI(0f));
        }
        tutorialText.text = newMessage;

        yield return StartCoroutine(FadeUI(1f));
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}