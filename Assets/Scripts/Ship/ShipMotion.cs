using UnityEngine;

public class ShipMotion : MonoBehaviour
{
    [Header("Ship Model")]
    public Transform shipModel;

    [Header("Very Subtle Sea Motion")]
    public float bobHeight = 0.015f;
    public float bobSpeed = 0.8f;

    public float rollAmount = 0.6f;
    public float rollSpeed = 0.6f;

    public float pitchAmount = 0.35f;
    public float pitchSpeed = 0.5f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private void Start()
    {
        if (shipModel == null)
        {
            shipModel =
                transform.Find("ShipModel");
        }

        if (shipModel == null)
        {
            Debug.LogError(
                "ShipMotion: ShipModel not found."
            );

            enabled = false;
            return;
        }

        startLocalPosition =
            shipModel.localPosition;

        startLocalRotation =
            shipModel.localRotation;
    }

    private void Update()
    {
        float bob =
            Mathf.Sin(
                Time.time * bobSpeed
            ) * bobHeight;

        float roll =
            Mathf.Sin(
                Time.time * rollSpeed
            ) * rollAmount;

        float pitch =
            Mathf.Sin(
                Time.time * pitchSpeed
            ) * pitchAmount;

        Vector3 targetPosition =
            startLocalPosition;

        targetPosition.y += bob;

        Quaternion targetRotation =
            startLocalRotation *
            Quaternion.Euler(
                pitch,
                0f,
                roll
            );

        shipModel.localPosition =
            targetPosition;

        shipModel.localRotation =
            targetRotation;
    }
}