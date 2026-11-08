
using System.Collections;
using UnityEngine;

public class RockMiningAnimation : MonoBehaviour
{
    [Header("Impact Shake")]
    public float shakeDuration = 0.18f;
    public float shakeStrength = 0.06f;
    public float shakeSpeed = 35f;

    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void PlayImpact()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float strength =
                Mathf.Lerp(shakeStrength, 0f,
                elapsed / shakeDuration);

            float offsetX =
                Mathf.Sin(elapsed * shakeSpeed) * strength;

            float offsetZ =
                Mathf.Cos(elapsed * shakeSpeed) * strength;

            transform.localPosition =
                originalPosition + new Vector3(offsetX, 0f, offsetZ);

            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }
}
