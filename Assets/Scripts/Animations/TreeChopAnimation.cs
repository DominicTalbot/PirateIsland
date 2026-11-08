
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeChopAnimation : MonoBehaviour
{
    [Header("Chopping Impact")]
    public float shakeAngle = 2f;
    public float shakeDuration = 0.15f;

    [Header("Falling")]
    public float fallAngle = 90f;
    public float fallDuration = 1.5f;

    [Header("Ground Impact")]
    public float bounceAngle = 6f;
    public float bounceDuration = 1f;
    public float bounceDamping = 4f;
    public float bounceFrequency = 15f;

    private Transform fallPivot;
    private Quaternion startingRotation;
    private Coroutine activeAnimation;
    private bool hasFallen;

    private void Awake()
    {
        CreateGroundPivot();
    }

    private void CreateGroundPivot()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogError("TREE | No renderers found.");
            return;
        }

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 pivotPosition = new Vector3(
            bounds.center.x,
            bounds.min.y,
            bounds.center.z
        );

        // Remember the original children.
        List<Transform> originalChildren =
            new List<Transform>();

        foreach (Transform child in transform)
        {
            originalChildren.Add(child);
        }

        // Create a pivot at the bottom of the tree.
        GameObject pivotObject = new GameObject("Tree_Fall_Pivot");

        fallPivot = pivotObject.transform;
        fallPivot.SetParent(transform, true);
        fallPivot.position = pivotPosition;
        fallPivot.rotation = transform.rotation;

        // Move the tree's existing objects under the pivot.
        foreach (Transform child in originalChildren)
        {
            child.SetParent(fallPivot, true);
        }

        startingRotation = fallPivot.localRotation;

        Debug.Log("TREE | Ground-level fall pivot created.");
    }

    public void PlayChopImpact()
    {
        if (hasFallen || fallPivot == null)
            return;

        if (activeAnimation != null)
            StopCoroutine(activeAnimation);

        activeAnimation = StartCoroutine(ChopShake());
    }

    private IEnumerator ChopShake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float strength =
                Mathf.Sin((elapsed / shakeDuration) * Mathf.PI);

            float angle = shakeAngle * strength;

            fallPivot.localRotation =
                startingRotation *
                Quaternion.Euler(angle, 0f, -angle);

            yield return null;
        }

        fallPivot.localRotation = startingRotation;
        activeAnimation = null;
    }

    public void Fall(Vector3 fallDirection)
    {
        if (hasFallen || fallPivot == null)
            return;

        if (activeAnimation != null)
            StopCoroutine(activeAnimation);

        activeAnimation = StartCoroutine(FallAnimation(fallDirection));
    }



    private IEnumerator FallAnimation(Vector3 fallDirection)
    {
        hasFallen = true;

        fallDirection.y = 0f;

        if (fallDirection.sqrMagnitude < 0.001f)
            fallDirection = Vector3.forward;

        fallDirection.Normalize();

        Vector3 rotationAxis =
            Vector3.Cross(Vector3.up, fallDirection).normalized;

        Quaternion initialRotation = fallPivot.rotation;

        Quaternion targetRotation =
            Quaternion.AngleAxis(fallAngle, rotationAxis) *
            initialRotation;

        // PHASE 1: Tree falls.
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / fallDuration);

            float smoothProgress = progress * progress;

            fallPivot.rotation = Quaternion.Slerp(
                initialRotation,
                targetRotation,
                smoothProgress
            );

            yield return null;
        }

        fallPivot.rotation = targetRotation;

        // PHASE 2: Tree bounces and settles after impact.
        elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;

            float bounce = bounceAngle *
                Mathf.Exp(-bounceDamping * elapsed) *
                Mathf.Sin(bounceFrequency * elapsed);

            Quaternion bounceRotation =
                Quaternion.AngleAxis(-bounce, rotationAxis) *
                targetRotation;

            fallPivot.rotation = bounceRotation;

            yield return null;
        }

        // Ensure the tree finishes completely still.
        fallPivot.rotation = targetRotation;

        activeAnimation = null;
    }


}
