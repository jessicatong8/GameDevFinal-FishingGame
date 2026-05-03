using TMPro;
using UnityEngine;

public class LevelUpUIText : MonoBehaviour
{
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject levelUpTextObject;
    private TextMeshProUGUI levelUpText;

    private void Awake()
    {
        if (levelUpPanel == null)
        {
            DebugLogger.Instance.LogError("LevelUpUIText: LevelUpPanel reference is not set in the inspector.");
        }
        if (levelUpTextObject != null && levelUpText == null)
        {
            levelUpText = levelUpTextObject.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        FishingManager.OnLevelUpPresentation += HandleLevelUpPresentation;
        FishingManager.OnReturnToGameplay += HandleReturnToGameplay;
    }

    private void Start()
    {
        levelUpPanel.SetActive(false);
    }

    private void OnDisable()
    {
        FishingManager.OnLevelUpPresentation -= HandleLevelUpPresentation;
        FishingManager.OnReturnToGameplay -= HandleReturnToGameplay;
    }

    private void HandleLevelUpPresentation()
    {
        levelUpPanel.SetActive(true);
        if (levelUpText != null)
        {
            levelUpText.text = "Congrats, you just got DRIPPED OUT! Maybe you're finally cool enough for drippier fish...";
        }
    }

    private void HandleReturnToGameplay()
    {
        levelUpPanel.SetActive(false);
    }
}

