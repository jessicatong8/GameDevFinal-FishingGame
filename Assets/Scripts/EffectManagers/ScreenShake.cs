using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    public bool start = false;
    public AnimationCurve shakeCurve;
    public float duration = 0.5f;

    void Update()
    {
        if (start)
        {
            start = false;
            StartCoroutine(Shake(duration));
        }
    }
    public IEnumerator Shake(float duration)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float currentStrength = shakeCurve.Evaluate(normalizedTime);

            float x = Random.Range(-1f, 1f) * currentStrength;
            float y = Random.Range(-1f, 1f) * currentStrength;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}