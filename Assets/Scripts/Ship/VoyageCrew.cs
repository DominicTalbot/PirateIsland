using UnityEngine;

public class VoyageCrew : MonoBehaviour
{

    public string crewId;
    
    public string crewName;

    public VoyageRole role;

    public Transform targetPosition;

    public float moveSpeed = 1.5f;

    private bool arrived;

    private float idleTimer;

    private Vector3 stationPosition;

    private void Start()
    {
        if (targetPosition == null)
        {
            return;
        }

        // Attach crew to the same moving hierarchy as their station.
        // This allows them to move with the ship while still
        // walking to their assigned position.
        if (targetPosition.parent != null)
        {
            transform.SetParent(
                targetPosition.parent,
                true
            );
        }
    }

    private void Update()
    {
        if (targetPosition == null)
        {
            return;
        }

        if (arrived)
        {
            DoRoleBehaviour();

            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition.position,
                moveSpeed * Time.deltaTime
            );

        Vector3 direction =
            targetPosition.position -
            transform.position;

        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        if (
            Vector3.Distance(
                transform.position,
                targetPosition.position
            ) < 0.2f
        )
        {
            arrived = true;

            stationPosition =
                transform.position;

            Debug.Log(
                role +
                " reached station"
            );
        }
    }

    private void DoRoleBehaviour()
    {
        idleTimer += Time.deltaTime;

        switch (role)
        {
            case VoyageRole.Captain:

                transform.localRotation =
                    Quaternion.Euler(
                        0f,
                        Mathf.Sin(idleTimer * 0.5f) * 5f,
                        0f
                    );

                break;


            case VoyageRole.Lookout:

                transform.localRotation =
                    Quaternion.Euler(
                        0f,
                        Mathf.Sin(idleTimer * 0.8f) * 15f,
                        0f
                    );

                break;


            case VoyageRole.Sailor:

                transform.localRotation =
                    Quaternion.Euler(
                        0f,
                        Mathf.Sin(idleTimer * 0.4f) * 3f,
                        0f
                    );

                break;
        }
    }
}