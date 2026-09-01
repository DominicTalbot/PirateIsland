using System.Collections.Generic;
using UnityEngine;

// ============================================================
// VOYAGE SHIP CONTROLLER
//
// This component owns ONLY the physical ship in VoyageScene.
//
// VoyageManager:
//     - owns persistent voyage state
//     - owns phase
//     - owns progress
//     - owns supplies/cargo
//
// MissionManager:
//     - owns mission resolution
//
// This controller:
//     - moves the visible ship
//     - follows waypoints
//     - updates physical voyage progress
// ============================================================

public class VoyageShipController : MonoBehaviour
{
    [Header("Movement")]
    [Min(0.01f)]
    public float travelSpeed = 2f;

    [Min(1f)]
    public float turnSpeed = 90f;

    [Min(0.01f)]
    public float arrivalDistance = 0.75f;

    [Min(0f)]
    public float approachDistance = 8f;


    [Header("Voyage Points")]
    public Transform startPoint;
    public Transform destinationPoint;
    public Transform homePoint;


    [Header("Outbound Route")]
    public List<Transform> outboundWaypoints =
        new List<Transform>();


    [Header("Return Route")]
    public List<Transform> returnWaypoints =
        new List<Transform>();

    public VoyageReturnTransition returnTransition;


    private VoyageData voyage;

    [Header("Crew On Ship")]
    public Transform[] crewSpawnPoints;
    public Transform crewFallbackPoint;

    public static bool PhysicalControllerActive { get; private set; }

    private bool missionStarted;
    private bool homeArrivalHandled;

    private VoyagePhase lastPhase;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (
            startPoint == null ||
            destinationPoint == null ||
            homePoint == null
        )
        {
            Debug.LogError(
                "VoyageShipController: " +
                "Start, Destination and Home points must be assigned."
            );

            enabled = false;
            return;
        }


        if (VoyageManager.Instance == null)
        {
            Debug.LogError(
                "VoyageShipController: " +
                "VoyageManager not found."
            );

            enabled = false;
            return;
        }


        voyage =
            VoyageManager.Instance
                .GetVoyageByShipId(
                    SceneNavigator.selectedShipId
                );


        if (voyage == null)
        {
            voyage =
                VoyageManager.Instance
                    .GetActiveVoyage();
        }


        if (voyage == null)
        {
            Debug.LogError(
                "VoyageShipController: " +
                "No active voyage found."
            );

            enabled = false;
            return;
        }

        if (voyage == null)
        {
            Debug.LogError(
                "VoyageShipController: " +
                "No active voyage found."
            );

            enabled = false;
            return;
        }

        Debug.Log(
            "SHIP CONTROLLER VOYAGE REF | " +
            voyage.voyageName +
            " | Ship ID: " +
            voyage.shipId +
            " | Phase: " +
            voyage.voyagePhase
        );

        PhysicalControllerActive = true;

        lastPhase =
            voyage.voyagePhase;


        // Physical voyage controller is now genuinely active.
        PhysicalControllerActive = true;


        lastPhase =
            voyage.voyagePhase;


        RestorePhysicalPosition();

        PlaceCrewOnShip();

        FaceCurrentTarget();


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "VOYAGE SHIP CONNECTED"
        );

        Debug.Log(
            "Voyage: " +
            voyage.voyageName
        );

        Debug.Log(
            "Phase: " +
            voyage.voyagePhase
        );

        Debug.Log(
            "Progress: " +
            voyage.progress
        );

        Debug.Log(
            "Waypoint: " +
            voyage.currentWaypointIndex
        );

        Debug.Log(
            "================================"
        );
    }

    private void OnDestroy()
    {
        PhysicalControllerActive = false;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {

        if (voyage == null)
        {
            return;
        }


        // Detect phase changes caused by MissionManager
        // or VoyageManager.
        if (voyage.voyagePhase != lastPhase)
        {
            HandlePhaseChanged(
                lastPhase,
                voyage.voyagePhase
            );

            lastPhase =
                voyage.voyagePhase;
        }


        switch (voyage.voyagePhase)
        {
            case VoyagePhase.LeavingIsland:

                // Island departure owns this phase.
                break;


            case VoyagePhase.TravellingToDestination:

                Navigate(false);
                break;


            case VoyagePhase.ApproachingDestination:

                Navigate(false);
                break;


            case VoyagePhase.Mission:

                // Ship stays at destination.
                break;


            case VoyagePhase.ReturningHome:

                Navigate(true);
                break;


            case VoyagePhase.ApproachingHome:

                Navigate(true);
                break;


            case VoyagePhase.Complete:

                // VoyageManager handles completion.
                break;
        }
    }


    // =========================================================
    // PHASE CHANGED
    // =========================================================

    private void HandlePhaseChanged(
        VoyagePhase oldPhase,
        VoyagePhase newPhase
    )
    {
        Debug.Log(
            "SHIP PHASE CHANGE: " +
            oldPhase +
            " -> " +
            newPhase
        );


        if (
            newPhase ==
            VoyagePhase.TravellingToDestination
        )
        {
            missionStarted = false;
            homeArrivalHandled = false;

            FaceCurrentTarget();
        }


        if (
            newPhase ==
            VoyagePhase.ReturningHome
        )
        {
            missionStarted = false;

            FaceCurrentTarget();
        }


        if (
            newPhase ==
            VoyagePhase.Complete
        )
        {
            Debug.Log(
                "VOYAGE SHIP: Voyage complete."
            );
        }
    }


    // =========================================================
    // NAVIGATION
    // =========================================================

    private void Navigate(
        bool returning
    )
    {
        Transform target =
            GetCurrentTarget(
                returning
            );


        if (target == null)
        {
            Debug.LogError(
                "VoyageShipController: " +
                "No valid navigation target."
            );

            return;
        }


        Vector3 targetPosition =
            target.position;


        // Keep the ship at its current waterline.
        targetPosition.y =
            transform.position.y;


        Vector3 direction =
            targetPosition -
            transform.position;


        float distance =
            direction.magnitude;


        // -----------------------------------------------------
        // ARRIVED AT CURRENT POINT
        // -----------------------------------------------------

        if (
            distance <=
            arrivalDistance
        )
        {
            transform.position =
                targetPosition;


            AdvanceRoute(
                returning
            );

            return;
        }


        // -----------------------------------------------------
        // APPROACHING
        // -----------------------------------------------------

        SetApproachPhase(
            returning,
            distance
        );


        // -----------------------------------------------------
        // TURN
        // -----------------------------------------------------

        SteerToward(
            direction
        );


        // -----------------------------------------------------
        // MOVE
        // -----------------------------------------------------

        transform.position +=
            transform.forward *
            travelSpeed *
            Time.deltaTime;


        // -----------------------------------------------------
        // SAVE PROGRESS
        // -----------------------------------------------------

        UpdateProgress(
            returning
        );
    }


    // =========================================================
    // TURN SHIP
    // =========================================================

    private void SteerToward(
        Vector3 direction
    )
    {
        direction.y = 0f;


        if (
            direction.sqrMagnitude <
            0.0001f
        )
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );


        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed *
                Time.deltaTime
            );
    }


    // =========================================================
    // FACE TARGET
    // =========================================================

    private void FaceCurrentTarget()
    {
        bool returning =
            voyage.voyagePhase ==
                VoyagePhase.ReturningHome ||
            voyage.voyagePhase ==
                VoyagePhase.ApproachingHome;


        Transform target =
            GetCurrentTarget(
                returning
            );


        if (target == null)
        {
            return;
        }


        Vector3 direction =
            target.position -
            transform.position;


        direction.y = 0f;


        if (
            direction.sqrMagnitude <
            0.0001f
        )
        {
            return;
        }


        transform.rotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );
    }


    // =========================================================
    // APPROACH PHASE
    // =========================================================

    private void SetApproachPhase(
        bool returning,
        float distance
    )
    {
        VoyagePhase travelling =
            returning
                ? VoyagePhase.ReturningHome
                : VoyagePhase.TravellingToDestination;


        VoyagePhase approaching =
            returning
                ? VoyagePhase.ApproachingHome
                : VoyagePhase.ApproachingDestination;


        if (
            distance <=
            approachDistance
            &&
            voyage.voyagePhase ==
            travelling
        )
        {
            VoyageManager.Instance
                .SetVoyagePhase(
                    voyage,
                    approaching
                );
        }
    }


    // =========================================================
    // CURRENT TARGET
    // =========================================================

    private Transform GetCurrentTarget(
        bool returning
    )
    {
        List<Transform> route =
            returning
                ? returnWaypoints
                : outboundWaypoints;


        int validIndex = 0;


        foreach (
            Transform waypoint
            in route
        )
        {
            if (waypoint == null)
            {
                continue;
            }


            if (
                validIndex ==
                voyage.currentWaypointIndex
            )
            {
                return waypoint;
            }


            validIndex++;
        }


        return returning
            ? homePoint
            : destinationPoint;
    }


    // =========================================================
    // COUNT VALID WAYPOINTS
    // =========================================================

    private int GetWaypointCount(
        bool returning
    )
    {
        int count = 0;


        foreach (
            Transform waypoint
            in returning
                ? returnWaypoints
                : outboundWaypoints
        )
        {
            if (waypoint != null)
            {
                count++;
            }
        }


        return count;
    }


    // =========================================================
    // ADVANCE ROUTE
    // =========================================================

    private void AdvanceRoute(
        bool returning
    )
    {
        int waypointCount =
            GetWaypointCount(
                returning
            );


        // -----------------------------------------------------
        // MORE WAYPOINTS
        // -----------------------------------------------------

        if (
            voyage.currentWaypointIndex <
            waypointCount
        )
        {
            voyage.currentWaypointIndex++;


            UpdateProgress(
                returning
            );


            FaceCurrentTarget();


            Debug.Log(
                "SHIP REACHED WAYPOINT " +
                voyage.currentWaypointIndex
            );


            return;
        }


        // -----------------------------------------------------
        // RETURNED HOME
        // -----------------------------------------------------

        if (returning)
        {
            ArriveHome();
            return;
        }


        // -----------------------------------------------------
        // REACHED DESTINATION
        // -----------------------------------------------------

        VoyageManager.Instance
            .SetVoyageProgress(
                voyage,
                70f
            );


        VoyageManager.Instance
            .SetVoyagePhase(
                voyage,
                VoyagePhase.Mission
            );


        BeginMission();
    }


    // =========================================================
    // BEGIN MISSION
    // =========================================================

    private void BeginMission()
    {
        if (missionStarted)
        {
            return;
        }


        missionStarted = true;


        Debug.Log(
            "SHIP ARRIVED AT MISSION DESTINATION"
        );


        if (
            MissionManager.Instance !=
            null
        )
        {
            MissionManager.Instance
                .BeginMissionWork();
        }
        else
        {
            Debug.LogWarning(
                "MissionManager not found. " +
                "Ship will remain at destination."
            );
        }
    }


    // =========================================================
    // ARRIVE HOME
    // =========================================================

    private void ArriveHome()
    {
        if (homeArrivalHandled)
        {
            return;
        }


        homeArrivalHandled = true;


        transform.position =
            homePoint.position;


        Debug.Log(
            "SHIP ARRIVED HOME"
        );

        if (returnTransition != null)
        {
            returnTransition.BeginDocking();
        }


        VoyageManager.Instance
            .CompleteVoyage(
                voyage
            );

    }


    // =========================================================
    // PROGRESS
    // =========================================================

    private void UpdateProgress(
        bool returning
    )
    {
        float routeProgress =
            GetRouteProgress(
                returning
            );


        float voyageProgress =
            returning
                ? Mathf.Lerp(
                    70f,
                    100f,
                    routeProgress
                )
                : Mathf.Lerp(
                    0f,
                    70f,
                    routeProgress
                );


        VoyageManager.Instance
            .SetVoyageProgress(
                voyage,
                voyageProgress
            );
    }


    // =========================================================
    // ROUTE PROGRESS
    // =========================================================

    private float GetRouteProgress(
        bool returning
    )
    {
        List<Transform> route =
            returning
                ? returnWaypoints
                : outboundWaypoints;


        Vector3 previous =
            returning
                ? destinationPoint.position
                : startPoint.position;


        float totalLength = 0f;
        float remainingLength = 0f;


        int index = 0;
        bool currentSegmentFound = false;


        foreach (
            Transform waypoint
            in route
        )
        {
            if (waypoint == null)
            {
                continue;
            }


            float segmentLength =
                Vector3.Distance(
                    previous,
                    waypoint.position
                );


            totalLength +=
                segmentLength;


            if (
                index >=
                voyage.currentWaypointIndex
            )
            {
                if (
                    index ==
                    voyage.currentWaypointIndex
                )
                {
                    remainingLength +=
                        Vector3.Distance(
                            transform.position,
                            waypoint.position
                        );
                }
                else
                {
                    remainingLength +=
                        segmentLength;
                }


                currentSegmentFound = true;
            }


            previous =
                waypoint.position;


            index++;
        }


        Vector3 endpoint =
            returning
                ? homePoint.position
                : destinationPoint.position;


        totalLength +=
            Vector3.Distance(
                previous,
                endpoint
            );


        if (currentSegmentFound)
        {
            remainingLength +=
                Vector3.Distance(
                    previous,
                    endpoint
                );
        }
        else
        {
            remainingLength +=
                Vector3.Distance(
                    transform.position,
                    endpoint
                );
        }


        if (
            totalLength <=
            0.001f
        )
        {
            return 1f;
        }


        return Mathf.Clamp01(
            1f -
            (
                remainingLength /
                totalLength
            )
        );
    }


    // =========================================================
    // RESTORE PHYSICAL POSITION
    // =========================================================

    private void RestorePhysicalPosition()
    {
        bool returning =
            voyage.voyagePhase ==
                VoyagePhase.ReturningHome ||
            voyage.voyagePhase ==
                VoyagePhase.ApproachingHome;


        float normalized;


        if (returning)
        {
            normalized =
                Mathf.InverseLerp(
                    70f,
                    100f,
                    voyage.progress
                );
        }
        else
        {
            normalized =
                Mathf.InverseLerp(
                    0f,
                    70f,
                    voyage.progress
                );
        }


        PlaceAlongRoute(
            returning,
            normalized
        );
    }


    // =========================================================
    // PLACE SHIP ALONG ROUTE
    // =========================================================

    private void PlaceAlongRoute(
        bool returning,
        float normalized
    )
    {
        List<Vector3> points =
            new List<Vector3>();


        points.Add(
            returning
                ? destinationPoint.position
                : startPoint.position
        );


        foreach (
            Transform waypoint
            in returning
                ? returnWaypoints
                : outboundWaypoints
        )
        {
            if (waypoint != null)
            {
                points.Add(
                    waypoint.position
                );
            }
        }


        points.Add(
            returning
                ? homePoint.position
                : destinationPoint.position
        );


        float totalLength = 0f;


        for (
            int i = 1;
            i < points.Count;
            i++
        )
        {
            totalLength +=
                Vector3.Distance(
                    points[i - 1],
                    points[i]
                );
        }


        if (
            totalLength <=
            0.001f
        )
        {
            transform.position =
                points[0];

            return;
        }


        float distanceAlongRoute =
            totalLength *
            Mathf.Clamp01(
                normalized
            );


        voyage.currentWaypointIndex =
            0;


        for (
            int i = 1;
            i < points.Count;
            i++
        )
        {
            float segmentLength =
                Vector3.Distance(
                    points[i - 1],
                    points[i]
                );


            if (
                distanceAlongRoute <=
                segmentLength
            )
            {
                float t =
                    segmentLength <=
                    0.001f
                        ? 1f
                        : distanceAlongRoute /
                          segmentLength;


                transform.position =
                    Vector3.Lerp(
                        points[i - 1],
                        points[i],
                        t
                    );


                voyage.currentWaypointIndex =
                    Mathf.Min(
                        i - 1,
                        GetWaypointCount(
                            returning
                        )
                    );


                return;
            }


            distanceAlongRoute -=
                segmentLength;
        }


        transform.position =
            points[points.Count - 1];
    }

    // =========================================================
    // PLACE SELECTED CREW ON SHIP
    // =========================================================

    private void PlaceCrewOnShip()
    {
        CrewMovement[] persistentCrew =
            FindObjectsByType<CrewMovement>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        int spawnIndex = 0;

        foreach (
            CrewMovement crew
            in persistentCrew
        )
        {
            if (crew == null)
            {
                continue;
            }

            /*
             * Only crew assigned to THIS voyage.
             */
            if (
                crew.crewData == null ||
                !crew.crewData.isOnVoyage ||
                crew.crewData.assignedShipId != voyage.shipId
            )
            {
                continue;
            }

            /*
             * Make sure the crew is visible.
             */
            crew.gameObject.SetActive(true);

            if (crew.visual != null)
            {
                crew.visual.gameObject.SetActive(true);
            }

            /*
             * Stop normal island behaviour.
             */
            crew.currentJob =
                CrewMovement.CrewJob.Voyage;

            crew.StopWorking();

            /*
             * Choose a spawn position.
             */
            Transform spawnPoint = null;

            if (
                crewSpawnPoints != null &&
                spawnIndex < crewSpawnPoints.Length
            )
            {
                spawnPoint =
                    crewSpawnPoints[spawnIndex];
            }

            if (spawnPoint != null)
            {
                /*
                 * IMPORTANT:
                 *
                 * Parent the crew to the physical ship.
                 * This means they travel with the ship.
                 */
                crew.transform.SetParent(
                    transform,
                    true
                );

                crew.transform.position =
                    spawnPoint.position;

                crew.transform.rotation =
                    spawnPoint.rotation;

                spawnIndex++;
            }
            else if (crewFallbackPoint != null)
            {
                crew.transform.SetParent(
                    transform,
                    true
                );

                crew.transform.position =
                    crewFallbackPoint.position;

                crew.transform.rotation =
                    crewFallbackPoint.rotation;
            }

            Debug.Log(
                "CREW PLACED ON VOYAGE SHIP: " +
                crew.name +
                " | Ship: " +
                voyage.shipId
            );
        }
    }
}