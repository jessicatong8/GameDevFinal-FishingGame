using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve shakeCurve;
    private Vector3 originalPos;
    private bool isConstantShaking = false;
    private float constantStrength = 0f;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void TriggerLargeShake(float duration, float intensity)
    {
        StartCoroutine(Shake(duration, intensity));
    }

    public IEnumerator Shake(float duration, float intensity)
    {
        originalPos = transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float currentStrength = shakeCurve.Evaluate(normalizedTime) * intensity;

            ApplyShake(currentStrength);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    public void SetConstantShake(bool active, float strength = 0.05f)
    {
        originalPos = transform.localPosition;
        isConstantShaking = active;
        constantStrength = strength;
    }

    void Update()
    {
        if (isConstantShaking)
        {
            ApplyShake(constantStrength);
        }
    }

    private void ApplyShake(float strength)
    {
        float x = Random.Range(-1f, 1f) * strength;
        float y = Random.Range(-1f, 1f) * strength;
        transform.localPosition = originalPos + new Vector3(x, y, 0);
    }
}