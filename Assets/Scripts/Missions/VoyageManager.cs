using System;
using System.Collections.Generic;
using UnityEngine;

// VoyageManager owns the logical/persistent voyage.
// VoyageShipController owns the physical ship when Voyage Scene is open.
public class VoyageManager : MonoBehaviour
{
    public static VoyageManager Instance;

    public List<VoyageData> activeVoyages =
        new List<VoyageData>();


    [Header("Background Voyage Timing")]

    public float outboundTravelDuration = 95f;

    public float returnTravelDuration = 59f;


    [Header("Approach")]

    [Range(0f, 1f)]
    public float destinationApproachPercent = 0.9f;

    [Range(0f, 1f)]
    public float homeApproachPercent = 0.9f;


    [Header("Supplies")]

    public float supplyConsumptionInterval = 10f;

    public int suppliesConsumedPerInterval = 1;


    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    private void Update()
    {
        for (
            int i = activeVoyages.Count - 1;
            i >= 0;
            i--
        )
        {
            VoyageData voyage =
                activeVoyages[i];

            if (voyage == null)
            {
                activeVoyages.RemoveAt(i);
                continue;
            }


            /*
             * When VoyageScene is open,
             * VoyageShipController controls
             * the visible ship.
             *
             * VoyageManager still owns the
             * persistent timestamps.
             */

            if (
                !VoyageShipController.PhysicalControllerActive
            )
            {
                UpdateBackgroundVoyage(voyage);
            }


            HandleVoyageSupplies(voyage);


            SyncVoyageToShip(voyage);
        }
    }


    // =========================================================
    // BACKGROUND VOYAGE
    // =========================================================

    private void UpdateBackgroundVoyage(
        VoyageData voyage
    )
    {
        long now =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();


        switch (voyage.voyagePhase)
        {
            case VoyagePhase.LeavingIsland:

                /*
                 * LeavingIsland is normally controlled
                 * by the physical ship.
                 *
                 * Once the ship has actually left
                 * the island, something should call:
                 *
                 * StartOutboundTravel(voyage)
                 *
                 * which changes the voyage into
                 * TravellingToDestination.
                 */

                break;


            case VoyagePhase.TravellingToDestination:

                UpdateOutboundTravel(
                    voyage,
                    now
                );

                break;


            case VoyagePhase.ApproachingDestination:

                UpdateDestinationApproach(
                    voyage,
                    now
                );

                break;


            case VoyagePhase.Mission:

                HandleBackgroundMission(
                    voyage
                );

                break;


            case VoyagePhase.ReturningHome:

                UpdateReturnTravel(
                    voyage,
                    now
                );

                break;


            case VoyagePhase.ApproachingHome:

                UpdateHomeApproach(
                    voyage,
                    now
                );

                break;


            case VoyagePhase.Complete:

                break;
        }
    }


    // =========================================================
    // OUTBOUND TRAVEL
    // =========================================================

    public void StartOutboundTravel(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        long now =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();


        voyage.outboundStartTime =
            now;


        voyage.outboundCompletionTime =
            now +
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    outboundTravelDuration
                )
            );


        voyage.progress =
            5f;


        /*
         * Make absolutely sure the phase
         * is correct.
         */

        voyage.voyagePhase =
            VoyagePhase.TravellingToDestination;


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "OUTBOUND TRAVEL STARTED"
        );

        Debug.Log(
            "Voyage: " +
            voyage.voyageName
        );

        Debug.Log(
            "Duration: " +
            outboundTravelDuration +
            " seconds"
        );

        Debug.Log(
            "Completion: " +
            voyage.outboundCompletionTime
        );

        Debug.Log(
            "================================"
        );


        SyncVoyageToShip(voyage);
    }


    private void UpdateOutboundTravel(
        VoyageData voyage,
        long now
    )
    {
        if (
            voyage.outboundCompletionTime <= 0
        )
        {
            StartOutboundTravel(voyage);

            return;
        }


        long start =
            voyage.outboundStartTime;

        long end =
            voyage.outboundCompletionTime;


        long duration =
            end - start;


        long elapsed =
            now - start;


        float progress =
            duration > 0
                ? Mathf.Clamp01(
                    (float)elapsed /
                    duration
                )
                : 1f;


        voyage.progress =
            Mathf.Lerp(
                5f,
                90f,
                progress
            );


        if (now >= end)
        {
            voyage.progress =
                90f;


            SetVoyagePhase(
                voyage,
                VoyagePhase.ApproachingDestination
            );


            voyage.approachStartTime =
                now;


            voyage.approachCompletionTime =
                now +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        outboundTravelDuration *
                        0.1f
                    )
                );
        }
    }


    // =========================================================
    // DESTINATION APPROACH
    // =========================================================

    private void UpdateDestinationApproach(
        VoyageData voyage,
        long now
    )
    {
        if (
            voyage.approachCompletionTime <= 0
        )
        {
            voyage.approachStartTime =
                now;

            voyage.approachCompletionTime =
                now +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        outboundTravelDuration *
                        0.1f
                    )
                );
        }


        long duration =
            voyage.approachCompletionTime -
            voyage.approachStartTime;


        long elapsed =
            now -
            voyage.approachStartTime;


        float progress =
            duration > 0
                ? Mathf.Clamp01(
                    (float)elapsed /
                    duration
                )
                : 1f;


        voyage.progress =
            Mathf.Lerp(
                90f,
                100f,
                progress
            );


        if (
            now >=
            voyage.approachCompletionTime
        )
        {
            voyage.progress =
                100f;


            SetVoyagePhase(
                voyage,
                VoyagePhase.Mission
            );


            StartMission(voyage);
        }
    }


    // =========================================================
    // RETURN TRAVEL
    // =========================================================

    public void StartReturnTravel(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        long now =
            DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();


        voyage.returnStartTime =
            now;


        voyage.returnCompletionTime =
            now +
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    returnTravelDuration
                )
            );


        voyage.progress =
            90f;


        voyage.voyagePhase =
            VoyagePhase.ReturningHome;


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "RETURN TRAVEL STARTED"
        );

        Debug.Log(
            "Voyage: " +
            voyage.voyageName
        );

        Debug.Log(
            "Duration: " +
            returnTravelDuration +
            " seconds"
        );

        Debug.Log(
            "Completion: " +
            voyage.returnCompletionTime
        );

        Debug.Log(
            "================================"
        );


        SyncVoyageToShip(voyage);
    }


    private void UpdateReturnTravel(
        VoyageData voyage,
        long now
    )
    {
        if (
            voyage.returnCompletionTime <= 0
        )
        {
            StartReturnTravel(voyage);

            return;
        }


        long start =
            voyage.returnStartTime;

        long end =
            voyage.returnCompletionTime;


        long duration =
            end - start;


        long elapsed =
            now - start;


        float progress =
            duration > 0
                ? Mathf.Clamp01(
                    (float)elapsed /
                    duration
                )
                : 1f;


        voyage.progress =
            Mathf.Lerp(
                90f,
                10f,
                progress
            );


        if (now >= end)
        {
            voyage.progress =
                10f;


            SetVoyagePhase(
                voyage,
                VoyagePhase.ApproachingHome
            );


            voyage.approachStartTime =
                now;


            voyage.approachCompletionTime =
                now +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        returnTravelDuration *
                        0.1f
                    )
                );
        }
    }


    // =========================================================
    // HOME APPROACH
    // =========================================================

    private void UpdateHomeApproach(
        VoyageData voyage,
        long now
    )
    {
        if (
            voyage.approachCompletionTime <= 0
        )
        {
            voyage.approachStartTime =
                now;

            voyage.approachCompletionTime =
                now +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        returnTravelDuration *
                        0.1f
                    )
                );
        }


        long duration =
            voyage.approachCompletionTime -
            voyage.approachStartTime;


        long elapsed =
            now -
            voyage.approachStartTime;


        float progress =
            duration > 0
                ? Mathf.Clamp01(
                    (float)elapsed /
                    duration
                )
                : 1f;


        voyage.progress =
            Mathf.Lerp(
                10f,
                100f,
                progress
            );


        if (
            now >=
            voyage.approachCompletionTime
        )
        {
            voyage.progress =
                100f;


            SetVoyagePhase(
                voyage,
                VoyagePhase.Complete
            );


            CompleteVoyage(voyage);
        }
    }


    // =========================================================
    // MISSION
    // =========================================================

    private void StartMission(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "VOYAGE ARRIVED AT DESTINATION"
        );

        Debug.Log(
            "MISSION STARTING: " +
            voyage.voyageName
        );

        Debug.Log(
            "================================"
        );


        if (
            MissionManager.Instance != null
        )
        {
            MissionManager.Instance
                .BeginMissionWork();
        }
    }


    private bool IsMissionTimeComplete(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return false;
        }


        if (
            voyage.missionCompletionTime <= 0
        )
        {
            return false;
        }


        long now =
            DateTimeOffset.UtcNow
                .ToUnixTimeMilliseconds();


        return now >=
            voyage.missionCompletionTime;
    }


    private void HandleBackgroundMission(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        if (
            !IsMissionTimeComplete(
                voyage
            )
        )
        {
            return;
        }


        Debug.Log(
            "MISSION TIME COMPLETE: " +
            voyage.voyageName
        );


        /*
         * MissionManager handles the result.
         */

        if (
            MissionManager.Instance != null
        )
        {
            MissionManager.Instance
                .FinishMission();
        }
        else
        {
            SetVoyagePhase(
                voyage,
                VoyagePhase.ReturningHome
            );
        }
    }


    // =========================================================
    // PHASE
    // =========================================================

    public void SetVoyagePhase(
        VoyageData voyage,
        VoyagePhase phase
    )
    {
        if (voyage == null)
        {
            return;
        }


        if (
            voyage.voyagePhase ==
            phase
        )
        {
            return;
        }


        VoyagePhase oldPhase =
            voyage.voyagePhase;


        voyage.voyagePhase =
            phase;


        Debug.Log(
            "VOYAGE PHASE: " +
            voyage.voyageName +
            " | " +
            oldPhase +
            " -> " +
            phase
        );


        /*
         * Start persistent clocks exactly
         * when the phase changes.
         */

        if (
            phase ==
            VoyagePhase.TravellingToDestination
        )
        {
            if (
                voyage.outboundStartTime <= 0
            )
            {
                StartOutboundTravel(
                    voyage
                );
            }
        }


        if (
            phase ==
            VoyagePhase.ReturningHome
        )
        {
            if (
                voyage.returnStartTime <= 0
            )
            {
                StartReturnTravel(
                    voyage
                );
            }
        }


        SyncVoyageToShip(voyage);
    }


    public void SetVoyageProgress(
        VoyageData voyage,
        float progress
    )
    {
        if (voyage == null)
        {
            return;
        }


        voyage.progress =
            Mathf.Clamp(
                progress,
                0f,
                100f
            );


        SyncVoyageToShip(voyage);
    }


    // =========================================================
    // SUPPLIES
    // =========================================================

    private void HandleVoyageSupplies(
        VoyageData voyage
    )
    {
        if (
            voyage.voyagePhase ==
                VoyagePhase.LeavingIsland ||
            voyage.voyagePhase ==
                VoyagePhase.Mission ||
            voyage.voyagePhase ==
                VoyagePhase.Complete
        )
        {
            return;
        }


        voyage.supplyTimer +=
            Time.deltaTime;


        if (
            voyage.supplyTimer <
            supplyConsumptionInterval
        )
        {
            return;
        }


        voyage.supplyTimer =
            0f;


        if (
            voyage.supplies <= 0
        )
        {
            voyage.needsAttention =
                true;

            return;
        }


        int amount =
            Mathf.Min(
                suppliesConsumedPerInterval,
                voyage.supplies
            );


        voyage.supplies -=
            amount;


        if (
            voyage.supplies <= 0
        )
        {
            voyage.supplies =
                0;

            voyage.needsAttention =
                true;
        }
    }


    public bool ConsumeSupplies(
        VoyageData voyage,
        int amount
    )
    {
        if (
            voyage == null ||
            amount <= 0
        )
        {
            return false;
        }


        if (
            voyage.supplies <
            amount
        )
        {
            return false;
        }


        voyage.supplies -=
            amount;


        SyncVoyageToShip(voyage);


        return true;
    }


    public void AddSupplies(
        VoyageData voyage,
        int amount
    )
    {
        if (
            voyage == null ||
            amount <= 0
        )
        {
            return;
        }


        voyage.supplies +=
            amount;


        SyncVoyageToShip(voyage);
    }


    // =========================================================
    // CARGO
    // =========================================================

    public bool AddCargo(
        VoyageData voyage,
        CargoType type,
        int amount
    )
    {
        if (
            voyage == null ||
            amount <= 0
        )
        {
            return false;
        }


        int cargoUsed =
            GetCargoUsed(voyage);


        int availableSpace =
            voyage.cargoCapacity -
            cargoUsed;


        if (availableSpace <= 0)
        {
            Debug.Log(
                "Cargo hold is full."
            );

            return false;
        }


        int amountToAdd =
            Mathf.Min(
                amount,
                availableSpace
            );


        CargoStack existingStack =
            null;


        foreach (
            CargoStack stack
            in voyage.cargo
        )
        {
            if (
                stack != null &&
                stack.type == type
            )
            {
                existingStack =
                    stack;

                break;
            }
        }


        if (existingStack != null)
        {
            existingStack.amount +=
                amountToAdd;
        }
        else
        {
            voyage.cargo.Add(
                new CargoStack(
                    type,
                    amountToAdd
                )
            );
        }


        SyncVoyageToShip(voyage);


        return true;
    }


    public int GetCargoUsed(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return 0;
        }


        int total = 0;


        foreach (
            CargoStack stack
            in voyage.cargo
        )
        {
            if (stack == null)
            {
                continue;
            }


            total +=
                stack.amount;
        }


        return total;
    }


    public void PrintCargo(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        Debug.Log(
            "----- VOYAGE CARGO -----"
        );


        foreach (
            CargoStack stack
            in voyage.cargo
        )
        {
            if (stack == null)
            {
                continue;
            }


            Debug.Log(
                stack.type +
                ": " +
                stack.amount
            );
        }


        Debug.Log(
            "Cargo Used: " +
            GetCargoUsed(voyage) +
            "/" +
            voyage.cargoCapacity
        );
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    public void CompleteVoyage(
        VoyageData voyage
    )
    {
        if (voyage == null)
        {
            return;
        }


        voyage.progress =
            100f;


        voyage.voyagePhase =
            VoyagePhase.Complete;


        if (
            ShipManager.Instance != null
        )
        {
            ShipState ship =
                ShipManager.Instance.GetShip(
                    voyage.shipId
                );


            if (ship != null)
            {
                ship.onVoyage =
                    false;

                ship.voyageProgress =
                    100f;

                ship.voyagePhase =
                    VoyagePhase.Complete;

                ship.supplies =
                    voyage.supplies;

                ship.cargo =
                    voyage.cargo;
            }
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "VOYAGE COMPLETE: " +
            voyage.voyageName
        );

        Debug.Log(
            "================================"
        );


        SyncVoyageToShip(voyage);


        if (
            MissionManager.Instance != null
        )
        {
            MissionManager.Instance
                .CompleteVoyageReturn();
        }
    }


    // =========================================================
    // LOOKUP
    // =========================================================

    public VoyageData GetVoyageByShipId(
        string shipId
    )
    {
        foreach (
            VoyageData voyage
            in activeVoyages
        )
        {
            if (voyage == null)
            {
                continue;
            }


            if (
                voyage.shipId ==
                shipId
            )
            {
                return voyage;
            }
        }


        return null;
    }


    public VoyageData GetActiveVoyage()
    {
        if (
            activeVoyages.Count == 0
        )
        {
            return null;
        }


        return activeVoyages[0];
    }


    // =========================================================
    // REBUILD
    // =========================================================

    public void RebuildActiveVoyages()
    {
        activeVoyages.Clear();


        if (
            ShipManager.Instance == null
        )
        {
            return;
        }


        foreach (
            ShipState ship
            in ShipManager.Instance.ships
        )
        {
            if (
                ship == null ||
                !ship.onVoyage
            )
            {
                continue;
            }


            VoyageData voyage =
    new VoyageData();

            voyage.voyageName =
                ship.destinationName;

            voyage.shipId =
                ship.shipId;

            voyage.progress =
                ship.voyageProgress;

            voyage.voyagePhase =
                ship.voyagePhase;

            voyage.supplies =
                ship.supplies;

            voyage.cargoCapacity =
                ship.cargoCapacity;

            voyage.cargo =
                ship.cargo;

            voyage.crewCount =
                ship.crewCount;

            voyage.currentWaypointIndex =
                ship.currentWaypointIndex;


            // =========================================================
            // RESTORE VOYAGE TIMELINES
            // =========================================================

            voyage.outboundStartTime =
                ship.outboundStartTime;

            voyage.outboundCompletionTime =
                ship.outboundCompletionTime;

            voyage.missionStartTime =
                ship.missionStartTime;

            voyage.missionCompletionTime =
                ship.missionCompletionTime;

            voyage.returnStartTime =
                ship.returnStartTime;

            voyage.returnCompletionTime =
                ship.returnCompletionTime;

            voyage.approachStartTime =
                ship.approachStartTime;

            voyage.approachCompletionTime =
                ship.approachCompletionTime;

            voyage.missionDuration =
                ship.missionDuration;

            voyage.outcomeGenerated =
                ship.outcomeGenerated;

            voyage.missionSucceeded =
                ship.missionSucceeded;

            voyage.needsAttention =
                ship.needsAttention;

            activeVoyages.Add(voyage);
        }
    }


    public void RebuildSelectedVoyage()
    {
        if (
            ShipManager.Instance == null
        )
        {
            return;
        }


        string shipId =
            SceneNavigator.selectedShipId;


        if (
            string.IsNullOrEmpty(shipId)
        )
        {
            return;
        }


        ShipState ship =
            ShipManager.Instance.GetShip(
                shipId
            );


        if (
            ship == null ||
            !ship.onVoyage
        )
        {
            return;
        }


        VoyageData voyage =
            GetVoyageByShipId(shipId);


        if (voyage == null)
        {
            voyage =
                new VoyageData();


            voyage.voyageName =
                ship.destinationName;

            voyage.shipId =
                ship.shipId;

            voyage.progress =
                ship.voyageProgress;

            voyage.voyagePhase =
                ship.voyagePhase;

            voyage.supplies =
                ship.supplies;

            voyage.cargoCapacity =
                ship.cargoCapacity;

            voyage.cargo =
                ship.cargo;

            voyage.crewCount =
                ship.crewCount;

            voyage.currentWaypointIndex =
                ship.currentWaypointIndex;

            voyage.outboundStartTime =
    ship.outboundStartTime;

            voyage.outboundCompletionTime =
                ship.outboundCompletionTime;

            voyage.missionStartTime =
                ship.missionStartTime;

            voyage.missionCompletionTime =
                ship.missionCompletionTime;

            voyage.returnStartTime =
                ship.returnStartTime;

            voyage.returnCompletionTime =
                ship.returnCompletionTime;

            voyage.approachStartTime =
                ship.approachStartTime;

            voyage.approachCompletionTime =
                ship.approachCompletionTime;

            voyage.missionDuration =
                ship.missionDuration;

            voyage.outcomeGenerated =
                ship.outcomeGenerated;

            voyage.missionSucceeded =
                ship.missionSucceeded;

            voyage.needsAttention =
                ship.needsAttention;


            activeVoyages.Add(voyage);


            Debug.Log(
                "NEW VOYAGE CREATED FROM SHIP STATE: " +
                ship.shipName
            );
        }
        else
        {
            Debug.Log(
                "EXISTING LIVE VOYAGE REUSED: " +
                ship.shipName
            );
        }
    }


    // =========================================================
    // SYNC
    // =========================================================

    private void SyncVoyageToShip(
        VoyageData voyage
    )
    {
        if (
            voyage == null ||
            ShipManager.Instance == null
        )
        {
            return;
        }


        ShipState ship =
            ShipManager.Instance.GetShip(
                voyage.shipId
            );


        if (ship == null)
        {
            return;
        }


        ship.voyageProgress =
    voyage.progress;

        ship.voyagePhase =
            voyage.voyagePhase;

        ship.supplies =
            voyage.supplies;

        ship.cargo =
            voyage.cargo;

        ship.currentWaypointIndex =
            voyage.currentWaypointIndex;


        // =========================================================
        // SYNC VOYAGE TIMELINES
        // =========================================================

        ship.outboundStartTime =
            voyage.outboundStartTime;

        ship.outboundCompletionTime =
            voyage.outboundCompletionTime;

        ship.missionStartTime =
            voyage.missionStartTime;

        ship.missionCompletionTime =
            voyage.missionCompletionTime;

        ship.returnStartTime =
            voyage.returnStartTime;

        ship.returnCompletionTime =
            voyage.returnCompletionTime;

        ship.approachStartTime =
            voyage.approachStartTime;

        ship.approachCompletionTime =
            voyage.approachCompletionTime;

        ship.missionDuration =
            voyage.missionDuration;

        ship.outcomeGenerated =
            voyage.outcomeGenerated;

        ship.missionSucceeded =
            voyage.missionSucceeded;

        ship.needsAttention =
            voyage.needsAttention;
    }
}