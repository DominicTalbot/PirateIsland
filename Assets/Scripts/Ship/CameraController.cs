using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float distance = 15f;

    public float minDistance = 5f;
    public float maxDistance = 30f;

    public float rotationSpeed = 100f;

    float yaw = 0f;

    void LateUpdate()
    {
        if (target == null)
            return;

        HandlePCInput();
        HandleMobileInput();

        Quaternion rotation =
            Quaternion.Euler(
                25f,
                yaw,
                0
            );

        Vector3 offset =
            rotation *
            new Vector3(
                0,
                0,
                -distance
            );

        transform.position =
            target.position + offset;

        transform.LookAt(target);
    }

    void HandlePCInput()
    {
        if (
            Input.GetMouseButton(1)
        )
        {
            yaw +=
                Input.GetAxis(
                    "Mouse X"
                ) *
                rotationSpeed *
                Time.deltaTime;
        }

        distance -=
            Input.GetAxis(
                "Mouse ScrollWheel"
            ) * 10f;

        distance =
            Mathf.Clamp(
                distance,
                minDistance,
                maxDistance
            );
    }

    void HandleMobileInput()
    {
        if (
            Input.touchCount == 1
        )
        {
            Touch touch =
                Input.GetTouch(0);

            if (
                touch.phase ==
                TouchPhase.Moved
            )
            {
                yaw +=
                    touch.deltaPosition.x *
                    0.1f;
            }
        }

        if (
            Input.touchCount == 2
        )
        {
            Touch t0 =
                Input.GetTouch(0);

            Touch t1 =
                Input.GetTouch(1);

            Vector2 t0Prev =
                t0.position -
                t0.deltaPosition;

            Vector2 t1Prev =
                t1.position -
                t1.deltaPosition;

            float prevMagnitude =
                (
                    t0Prev -
                    t1Prev
                ).magnitude;

            float currentMagnitude =
                (
                    t0.position -
                    t1.position
                ).magnitude;

            float difference =
                currentMagnitude -
                prevMagnitude;

            distance -=
                difference *
                0.01f;

            distance =
                Mathf.Clamp(
                    distance,
                    minDistance,
                    maxDistance
                );
        }
    }
}