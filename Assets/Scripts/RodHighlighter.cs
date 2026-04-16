using UnityEngine;

public class RodHighlighter : MonoBehaviour
{
    private MeshRenderer rodRenderer;
    private Material normalMaterial;
    private Material highlightMaterial;

    private void OnEnable()
    {
        FishingManager.OnBite += ApplyHighlight;
        FishingManager.OnReel += RemoveHighlight;
        FishingManager.OnCast += RemoveHighlight;
        FishingManager.OnWiggle += ApplyRedLight;
        FishingManager.OffWiggle += RemoveHighlight;
    }

    private void OnDisable()
    {
        FishingManager.OnBite -= ApplyHighlight;
        FishingManager.OnReel -= RemoveHighlight;
        FishingManager.OnCast -= RemoveHighlight;
        FishingManager.OnWiggle -= ApplyRedLight;
        FishingManager.OffWiggle -= RemoveHighlight;
    }

    private void ApplyHighlight()
    {
        rodRenderer.material = highlightMaterial;
    }
    //maybe for wiggling - red light when wiggling
    private void ApplyRedLight()
    {
        rodRenderer.material = highlightMaterial;
    }

    private void RemoveHighlight()
    {
        rodRenderer.material = normalMaterial
    }
}
