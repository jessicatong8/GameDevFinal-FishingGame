using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LineRangeUI : MonoBehaviour
{
    public FishingManager fishManager;

    public Image exitingLineRangeIndicator;
    private Color originalColor;
    // public Slider tensionSlider;
    // public TextMeshProUGUI alertText;

    void Start()
    {
        originalColor = exitingLineRangeIndicator.color; // Store the original color of the indicator

    }

    void OnEnable()
    {
        FishingManager.ExitingLeftLineRange += HandleExitingLeftLineRange;
        FishingManager.ExitingRightLineRange += HandleExitingRightLineRange;
        FishingManager.InRange += HandleReset;
        FishingManager.OnEscaped += HandleReset; // Reset the indicator when the fish escapes

    }

    void OnDisable()
    {
        FishingManager.ExitingLeftLineRange -= HandleExitingLeftLineRange;
        FishingManager.ExitingRightLineRange -= HandleExitingRightLineRange;
        FishingManager.InRange -= HandleReset;
        FishingManager.OnEscaped -= HandleReset;
    }

    void HandleExitingLeftLineRange()
    {
        exitingLineRangeIndicator.color = Color.red; // Change the color to red to indicate the fish is exiting the left range
    }
    void HandleExitingRightLineRange()
    {
        exitingLineRangeIndicator.color = Color.red; // Change the color to red to indicate the fish is exiting the right range
    }
    void HandleReset()
    {
        exitingLineRangeIndicator.color = originalColor; // Change the color back to the original color to indicate the fish is back in range
    }

}
