using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Navigation")]
    public Transform destination;

    [Header("Engine")]
    public float maxSpeed = 7f;
    public float acceleration = 1.5f;
    public float drag = 0.5f;

    [Header("Steering")]
    public float rudderSpeed = 40f;
    public float maxRudderAngle = 35f;

    [Header("Debug")]
    public float currentSpeed;
    public float rudderAngle;

    float desiredHeading;

    void Update()
    {
        if (destination == null)
            return;

        UpdateNavigation();

        UpdateSteering();

        UpdateMovement();
    }

    void UpdateNavigation()
    {
        Vector3 direction =
            destination.position -
            transform.position;

        direction.y = 0;

        desiredHeading =
            Quaternion.LookRotation(direction)
            .eulerAngles.y;
    }

    void UpdateSteering()
    {
        float currentHeading =
            transform.eulerAngles.y;

        float delta =
            Mathf.DeltaAngle(
                currentHeading,
                desiredHeading);

        float targetRudder =
            Mathf.Clamp(
                delta,
                -maxRudderAngle,
                maxRudderAngle);

        rudderAngle =
            Mathf.MoveTowards(
                rudderAngle,
                targetRudder,
                rudderSpeed *
                Time.deltaTime);

        transform.Rotate(
            0,
            rudderAngle *
            currentSpeed *
            0.02f *
            Time.deltaTime,
            0);
    }

    void UpdateMovement()
    {
        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration *
                Time.deltaTime);

        currentSpeed -=
            drag *
            Time.deltaTime;

        currentSpeed =
            Mathf.Clamp(
                currentSpeed,
                0,
                maxSpeed);

        transform.position +=
            transform.forward *
            currentSpeed *
            Time.deltaTime;
    }
}