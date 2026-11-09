
using System.Collections;
using UnityEngine;

public class VegetationClearAnimation : MonoBehaviour
{
    [Header("Initial Shake")]
    public float shakeDuration = 0.5f;
    public float shakeAngle = 8f;
    public float shakeSpeed = 30f;

    [Header("Collapse")]
    public float collapseDuration = 0.8f;
    public float collapseDistance = 0.35f;
    public float shrinkAmount = 0.65f;

    private Transform[] foliage;
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;
    private Vector3[] originalScales;

    private bool isClearing;

    private void Awake()
    {
        int count = transform.childCount;

        foliage = new Transform[count];
        originalPositions = new Vector3[count];
        originalRotations = new Quaternion[count];
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Transform part = transform.GetChild(i);

            foliage[i] = part;
            originalPositions[i] = part.localPosition;
            originalRotations[i] = part.localRotation;
            originalScales[i] = part.localScale;
        }
    }

    public void ClearVegetation()
    {
        if (isClearing)
            return;

        isClearing = true;

        VegetationWindAnimation wind =
            GetComponent<VegetationWindAnimation>();

        if (wind != null)
            wind.enabled = false;

        StartCoroutine(ClearAnimation());
    }

    private IEnumerator ClearAnimation()
    {
        // Shake the vegetation as it is being cut
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float strength =
                1f - elapsed / shakeDuration;

            for (int i = 0; i < foliage.Length; i++)
            {
                float movement = Mathf.Sin(
                    elapsed * shakeSpeed + i
                ) * shakeAngle * strength;

                foliage[i].localRotation =
                    originalRotations[i] *
                    Quaternion.Euler(movement, 0f, movement);
            }

            yield return null;
        }

        // Collapse the foliage towards the ground
        elapsed = 0f;

        while (elapsed < collapseDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed / collapseDuration)
            );

            for (int i = 0; i < foliage.Length; i++)
            {
                Transform part = foliage[i];

                Vector3 direction = new Vector3(
                    Mathf.Sin(i * 2.4f),
                    0f,
                    Mathf.Cos(i * 2.4f)
                );

                part.localPosition =
                    originalPositions[i] +
                    direction * collapseDistance * progress;

                part.localRotation =
                    Quaternion.Slerp(
                        originalRotations[i],
                        originalRotations[i] *
                        Quaternion.Euler(65f, 0f, 35f),
                        progress
                    );

                part.localScale =
                    Vector3.Lerp(
                        originalScales[i],
                        originalScales[i] * shrinkAmount,
                        progress
                    );
            }

            yield return null;
        }

        // Remove the cleared vegetation
        Destroy(gameObject);
    }
}
