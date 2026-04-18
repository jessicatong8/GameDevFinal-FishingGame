using UnityEngine;

public class RodHighlighter : MonoBehaviour
{
    [SerializeField] private MeshRenderer rodRenderer;
    [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color lowTensionColor = Color.green;
    [SerializeField] private Color midTensionColor = Color.yellow;
    [SerializeField] private Color highTensionColor = Color.red;

    private Material normalMaterial;
    private Material runtimeHighlightMaterial;
    private bool isHighlightActive;
    private float maxTension = 1f;

    private void OnEnable()
    {
        FishingManager.OnBite += HandleBite;
        FishingManager.MaxTensionUpdated += HandleMaxTensionUpdated;
        FishingManager.CurrentTensionUpdated += HandleCurrentTensionUpdated;
        FishingManager.OnReturnToIdle += ClearHighlight;
        FishingManager.OnCaught += ClearHighlight;
        FishingManager.OnEscaped += ClearHighlight;
        FishingManager.OnCast += ClearHighlight;
        FishingManager.OnFishingGameStateChanged += HandleFishingGameStateChanged;

        ClearHighlight();
    }

    private void OnDisable()
    {
        FishingManager.OnBite -= HandleBite;
        FishingManager.MaxTensionUpdated -= HandleMaxTensionUpdated;
        FishingManager.CurrentTensionUpdated -= HandleCurrentTensionUpdated;
        FishingManager.OnReturnToIdle -= ClearHighlight;
        FishingManager.OnCaught -= ClearHighlight;
        FishingManager.OnEscaped -= ClearHighlight;
        FishingManager.OnCast -= ClearHighlight;
        FishingManager.OnFishingGameStateChanged -= HandleFishingGameStateChanged;

        ClearHighlight();
    }

    private void OnDestroy()
    {
        if (runtimeHighlightMaterial != null)
        {
            Destroy(runtimeHighlightMaterial);
        }
    }

    private void HandleBite()
    {
        ActivateHighlight();
        ApplyTensionColor(0f);
    }

    private void HandleMaxTensionUpdated(float newMaxTension)
    {
        maxTension = Mathf.Max(newMaxTension, 0.0001f);
    }

    private void HandleCurrentTensionUpdated(float currentTension)
    {
        if (!isHighlightActive)
        {
            return;
        }

        ApplyTensionColor(currentTension);
    }

    private void HandleFishingGameStateChanged(FishingManager.FishingGameState gameState)
    {
        if (gameState == FishingManager.FishingGameState.Idle)
        {
            ClearHighlight();
        }
    }

    private void ActivateHighlight()
    {
        if (rodRenderer == null)
        {
            return;
        }

        if (highlightMaterial == null)
        {
            Debug.LogWarning("RodHighlighter: Highlight material is not assigned.");
            return;
        }
        if (runtimeHighlightMaterial == null)
        {
            runtimeHighlightMaterial = new Material(highlightMaterial);
        }

        rodRenderer.material = runtimeHighlightMaterial;
        isHighlightActive = true;
    }

    private void ApplyTensionColor(float currentTension)
    {
        if (runtimeHighlightMaterial == null)
        {
            return;
        }

        float normalized = Mathf.Clamp01(currentTension / maxTension);
        Color tensionColor = normalized < 0.5f
            ? Color.Lerp(lowTensionColor, midTensionColor, normalized / 0.5f)
            : Color.Lerp(midTensionColor, highTensionColor, (normalized - 0.5f) / 0.5f);

        if (runtimeHighlightMaterial.HasProperty("_BaseColor"))
        {
            runtimeHighlightMaterial.SetColor("_BaseColor", tensionColor);
        }

        if (runtimeHighlightMaterial.HasProperty("_Color"))
        {
            runtimeHighlightMaterial.SetColor("_Color", tensionColor);
        }
    }

    private void ClearHighlight()
    {
        if (rodRenderer == null || normalMaterial == null)
        {
            return;
        }

        rodRenderer.material = normalMaterial;
        isHighlightActive = false;
        maxTension = 1f;
    }
}
