
using UnityEngine;

public class TreeWindAnimation : MonoBehaviour
{
    [Header("Wind Settings")]
    public float swaySpeed = 1.2f;
    public float swayAngle = 3f;
    public float secondarySway = 1.5f;

    private Transform[] foliageSections;
    private Quaternion[] startingRotations;
    private float[] phaseOffsets;

    private void Start()
    {
        // Find all foliage objects underneath this tree.
        Transform[] allChildren =
            GetComponentsInChildren<Transform>(true);

        System.Collections.Generic.List<Transform> foliage =
            new System.Collections.Generic.List<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name.StartsWith("Foliage_"))
            {
                foliage.Add(child);
            }
        }

        foliageSections = foliage.ToArray();

        startingRotations = new Quaternion[foliageSections.Length];
        phaseOffsets = new float[foliageSections.Length];

        for (int i = 0; i < foliageSections.Length; i++)
        {
            startingRotations[i] = foliageSections[i].localRotation;

            // Give each foliage section a slightly different movement.
            phaseOffsets[i] = i * 0.7f;
        }

        Debug.Log(
            "TREE WIND | Foliage sections found: " +
            foliageSections.Length
        );
    }

    private void Update()
    {
        if (foliageSections == null)
            return;

        for (int i = 0; i < foliageSections.Length; i++)
        {
            float sway = Mathf.Sin(
                Time.time * swaySpeed + phaseOffsets[i]
            ) * swayAngle;

            float secondary = Mathf.Sin(
                Time.time * swaySpeed * 0.7f + phaseOffsets[i]
            ) * secondarySway;

            Quaternion windRotation = Quaternion.Euler(
                secondary,
                0f,
                sway
            );

            foliageSections[i].localRotation =
                startingRotations[i] * windRotation;
        }
    }
}
