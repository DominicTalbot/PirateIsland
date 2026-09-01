using System.Collections;
using UnityEngine;

public class CrewMovement : MonoBehaviour
{
    [Header("Prefab Identity")]
    [Tooltip("If this scene prefab represents a persistent crew member, set the CrewId (e.g. crew_001).")]
    public string starterCrewId;

    [Header("Crew Identity")]
    public CrewData crewData;

    public enum CrewJob
    {
        Idle,
        Fishing,
        Building,
        Mission,
        Voyage
    }

    public Transform[] waypoints;

    public Transform assignedFishingSpot;

    public Transform visual;

    public float moveSpeed = 1.5f;

    public float hopHeight = 0.08f;

    public float hopSpeed = 10f;

    public float squashAmount = 0.15f;

    private Transform target;

    private Vector3 visualStartScale;

    private Vector3 visualStartPos;

    private bool boardedShip;

    public Transform voyageTarget;

    [HideInInspector]
    public bool assignedToMission;

    [HideInInspector]
    public CrewJob currentJob;

    private bool working;

    private bool atWorkSpot;

    private Transform workTarget;

    private bool movingToDock;

    /*
     * This remains true once this crew member
     * has boarded the ship and survived the
     * Island scene being unloaded.
     */
    private bool voyagePersistent;

    private Transform dockTarget;

    public Transform assignedBuilderSpot;

    private void Awake()
    {
        // Register as early as possible. If CrewManager hasn't been created yet,
        // wait a few frames for it to appear.
        StartCoroutine(RegisterWhenManagerReady());
    }

    private IEnumerator RegisterWhenManagerReady()
    {
        // =========================================================
        // VOYAGE VISUAL CLONE
        // =========================================================

        var voyageComp = GetComponent<VoyageCrew>();

        bool isVoyageClone =
            voyageComp != null &&
            gameObject.name.Contains("(Clone)");

        if (isVoyageClone)
        {
            Debug.Log(
                $"CrewMovement: Voyage visual clone detected - skipping registration for '{name}'."
            );

            yield break;
        }


        // =========================================================
        // WAIT FOR CREW MANAGER
        // =========================================================

        int maxFrames = 5;
        int i = 0;

        while (CrewManager.Instance == null && i < maxFrames)
        {
            i++;
            yield return null;
        }

        if (CrewManager.Instance == null)
        {
            Debug.LogWarning(
                "CrewMovement: CrewManager not ready, registration skipped for " +
                name
            );

            yield break;
        }


        // =========================================================
        // RECONNECT TO AUTHORITATIVE CREW DATA
        // =========================================================

        if (!string.IsNullOrWhiteSpace(starterCrewId))
        {
            bool needsReconnect =
                crewData == null ||
                string.IsNullOrWhiteSpace(crewData.crewId) ||
                crewData.crewId != starterCrewId;

            if (needsReconnect)
            {
                CrewData found =
                    CrewManager.Instance.GetCrewDataById(starterCrewId);

                if (found != null)
                {
                    crewData = found;

                    Debug.Log(
                        "CrewMovement: RECONNECTED TO AUTHORITATIVE DATA | " +
                        "GO: " + name +
                        " | ID: " + starterCrewId +
                        " | On Voyage: " + crewData.isOnVoyage
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "CrewMovement: NO CREW DATA FOUND | " +
                        "Starter ID: " + starterCrewId +
                        " | GO: " + name
                    );
                }
            }
        }


        // =========================================================
        // REGISTER
        // =========================================================

        bool registered =
            CrewManager.Instance.RegisterCrew(this);

        if (!registered)
        {
            yield break;
        }
    }
    private void Start()
    {
        // Keep visuals initialization in Start (still runs after Awake).
        if (visual != null)
        {
            visualStartScale =
                visual.localScale;

            visualStartPos =
                visual.localPosition;
        }
    }

    private void Update()
    {
        /*
         * Crew currently represented on the voyage ship.
         */
        if (
            currentJob ==
            CrewJob.Voyage
        )
        {
            AnimateWorkingIdle();

            return;
        }

        /*
         * No Island movement target.
         */
        if (target == null)
        {
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

        Vector3 direction =
            target.position -
            transform.position;

        direction.y = 0f;

        if (
            direction != Vector3.zero
        )
        {
            Quaternion lookRotation =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    8f * Time.deltaTime
                );
        }

        if (!working)
        {
            AnimateHop();
        }
        else
        {
            AnimateWorkingIdle();
        }

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance < 0.2f)
        {
            /*
             * Crew travelling to the ship dock.
             */
            if (movingToDock)
            {
                if (boardedShip)
                {
                    return;
                }

                boardedShip = true;

                /*
                 * Preserve this crew member across
                 * the Island -> Voyage scene change.
                 */
                if (!voyagePersistent)
                {
                    voyagePersistent = true;

                    currentJob = CrewJob.Voyage;

                    DontDestroyOnLoad(gameObject);

                    Debug.Log(
                        "CREW BOARDED SHIP: " +
                        name +
                        " | Job changed to VOYAGE"
                    );
                }

                /*
                 * Hide the Island representation.
                 */
                HideIslandRepresentation();

                /*
                 * Tell CrewManager this crew member
                 * has reached the ship.
                 */
                if (
                    CrewManager.Instance != null
                )
                {
                    CrewManager.Instance
                        .CrewReachedDock();
                }

                target = null;

                return;
            }

            /*
             * Fishing crew.
             */
            if (
                currentJob ==
                CrewJob.Fishing
            )
            {
                if (!atWorkSpot)
                {
                    atWorkSpot = true;

                    working = true;

                    if (crewData != null)
                    {
                        crewData.wasWorking = true;
                    }

                    if (
                        CrewManager.Instance != null
                    )
                    {
                        CrewManager.Instance
                            .FishermanStartedWork();
                    }
                }

                return;
            }

            /*
             * Building crew.
             */
            if (
                currentJob ==
                CrewJob.Building
            )
            {
                if (!atWorkSpot)
                {
                    atWorkSpot = true;

                    working = true;

                    if (crewData != null)
                    {
                        crewData.wasWorking = true;
                    }

                    if (
                        CrewManager.Instance != null
                    )
                    {
                        CrewManager.Instance
                            .BuilderStartedWork();

                        if (
                            CrewManager.Instance
                                .activeConstruction !=
                            null
                        )
                        {
                            CrewManager.Instance
                                .activeConstruction
                                .ConstructionStarted();
                        }
                    }
                }

                return;
            }

            PickNewTargetPublic();
        }
    }

    public string GetDebugState()
    {
        return
            currentJob +
            " | Mission:" +
            assignedToMission +
            " | VoyagePersistent:" +
            voyagePersistent;
    }

    void AnimateHop()
    {
        if (visual == null)
        {
            return;
        }

        float bounce =
            Mathf.Abs(
                Mathf.Sin(
                    Time.time *
                    hopSpeed
                )
            );

        Vector3 localPos =
            visualStartPos;

        localPos.y +=
            bounce *
            hopHeight;

        visual.localPosition =
            localPos;

        float squash =
            1f -
            (
                bounce *
                squashAmount
            );

        visual.localScale =
            new Vector3(
                visualStartScale.x *
                (2f - squash),

                visualStartScale.y *
                squash,

                visualStartScale.z *
                (2f - squash)
            );
    }

    void AnimateWorkingIdle()
    {
        if (visual == null)
        {
            return;
        }

        visual.localPosition =
            visualStartPos;

        visual.localScale =
            visualStartScale;
    }

    public void PickNewTarget()
    {
        if (
            waypoints == null ||
            waypoints.Length == 0
        )
        {
            target = null;

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                waypoints.Length
            );

        target =
            waypoints[
                randomIndex
            ];
    }

    public void PickNewTargetPublic()
    {
        PickNewTarget();
    }

    public void GoToDock(
        Transform dock
    )
    {
        currentJob =
            CrewJob.Mission;

        assignedToMission =
            true;

        movingToDock =
            true;

        boardedShip =
            false;

        target =
            dock;
    }

    public void ReturnToIsland()
    {
        currentJob =
            CrewJob.Idle;

        assignedToMission =
            false;

        movingToDock =
            false;

        working =
            false;

        atWorkSpot =
            false;

        assignedFishingSpot =
            null;

        assignedBuilderSpot =
            null;

        workTarget =
            null;

        target =
            null;

        boardedShip =
            false;

        voyagePersistent =
            false;

        /*
         * CrewData is the persistent source of truth.
         */
        if (crewData != null)
        {
            crewData.isOnVoyage =
                false;

            crewData.assignedShipId =
                "";
        }

        if (visual != null)
        {
            visual.gameObject.SetActive(true);
        }

        ShowIslandRepresentation();

        PickNewTargetPublic();
    }

    public void AssignFishingJob(Transform fishingSpot)
    {
        if (fishingSpot == null)
        {
            Debug.LogWarning(
                "Cannot assign fishing job. Fishing spot is null."
            );

            return;
        }

        assignedFishingSpot = fishingSpot;

        currentJob = CrewJob.Fishing;

        workTarget = fishingSpot;

        // ALWAYS make the fishing spot the movement target.
        target = fishingSpot;

        working = false;
        atWorkSpot = false;

        Debug.Log(
            "FISHING JOB ASSIGNED | " +
            name +
            " | Target: " +
            fishingSpot.name +
            " | Distance: " +
            Vector3.Distance(transform.position, fishingSpot.position)
        );
    }

    public void AssignBuilderJob(
    Transform builderSpot
    )
    {
        if (builderSpot == null)
        {
            Debug.LogWarning(
                "Cannot assign builder job. " +
                "Builder spot is null."
            );

            return;
        }

        assignedBuilderSpot =
            builderSpot;

        currentJob =
            CrewJob.Building;

        working =
            false;

        atWorkSpot =
            false;

        target =
            builderSpot;
    }

    public bool IsWorking()
    {
        return working;
    }

    public void StopWorking()
    {
        working =
            false;

        atWorkSpot =
            false;
    }

    public bool IsVoyagePersistent()
    {
        return voyagePersistent;
    }

    public void ReconnectToIsland(
    CrewManager manager
    )
    {
        if (manager == null)
        {
            return;
        }

        manager.RegisterCrew(this);

        if (crewData == null)
        {
            Debug.LogWarning(
                "CREW RECONNECT FAILED - No CrewData | " +
                name
            );

            return;
        }

        /*
         * CrewData is the source of truth.
         */

        if (crewData.isOnVoyage)
        {
            currentJob =
                CrewJob.Voyage;

            assignedToMission =
                true;

            HideIslandRepresentation();

            Debug.Log(
                "CREW RECONNECTED | " +
                crewData.crewName +
                " | VOYAGE"
            );

            return;
        }

        /*
         * Crew has returned to the island.
         * CrewManager.RestoreIslandState()
         * will restore the actual island job.
         */

        voyagePersistent =
            false;

        assignedToMission =
            false;

        ShowIslandRepresentation();

        Debug.Log(
            "CREW RECONNECTED TO ISLAND | " +
            crewData.crewName +
            " | Job will be restored from CrewData"
        );
    }

    public void HideIslandRepresentation()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    public void ShowIslandRepresentation()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    public void RestoreIslandRepresentation()
    {
        if (visual != null)
        {
            visual.gameObject.SetActive(true);
        }
    }

    public void HideIslandRepresentationAfterLoad()
    {
        HideIslandRepresentation();
    }

    public void SetWorkingState(bool state)
    {
        working = state;
        atWorkSpot = state;
    }
}