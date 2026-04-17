using UnityEngine;

public class RodHighlighter : MonoBehaviour
{
    private MeshRenderer rodRenderer;
    private Material normalMaterial;
    private Material highlightMaterial;

    private void OnEnable()
    {
        FishingManager.OnBite += ApplyHighlight;
        FishingManager.OnReelAttempt += RemoveHighlight;
        FishingManager.OnCast += RemoveHighlight;
    }

    private void OnDisable()
    {
        FishingManager.OnBite -= ApplyHighlight;
        FishingManager.OnReelAttempt -= RemoveHighlight;
        FishingManager.OnCast -= RemoveHighlight;
    }

    private void ApplyHighlight()
    {
        rodRenderer.material = highlightMaterial;
    }

    private void RemoveHighlight()
    {
        rodRenderer.material = normalMaterial;
    }
}
