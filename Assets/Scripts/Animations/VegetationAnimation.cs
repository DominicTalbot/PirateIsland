
using UnityEngine;

public class VegetationWindAnimation : MonoBehaviour
{
    [Header("Wind Settings")]
    public float swaySpeed = 1.2f;
    public float swayAngle = 4f;
    public float secondarySway = 2f;

    private Transform[] foliageParts;
    private Quaternion[] originalRotations;
    private float[] phaseOffsets;

    private void Start()
    {
        // Find all individual foliage pieces
        foliageParts = new Transform[transform.childCount];
        originalRotations = new Quaternion[transform.childCount];
        phaseOffsets = new float[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform part = transform.GetChild(i);

            foliageParts[i] = part;
            originalRotations[i] = part.localRotation;
            phaseOffsets[i] = Random.Range(0f, 6.28f);
        }
    }

    private void Update()
    {
        if (foliageParts == null)
            return;

        for (int i = 0; i < foliageParts.Length; i++)
        {
            float sway = Mathf.Sin(
                Time.time * swaySpeed + phaseOffsets[i]
            ) * swayAngle;

            float secondary = Mathf.Cos(
                Time.time * swaySpeed * 0.7f + phaseOffsets[i]
            ) * secondarySway;

            foliageParts[i].localRotation =
                originalRotations[i] *
                Quaternion.Euler(sway, 0f, secondary);
        }
    }
}
