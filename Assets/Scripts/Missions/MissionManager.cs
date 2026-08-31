using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Mission State")]

    public MissionData currentMission;

    public bool missionInProgress;

    public float missionTimer;

    public int crewAssigned = 0;

    [Header("Ship")]

    public ShipMover shipMover;

    [Header("Mission References")]

    public MissionData nearbyWreckMission;

    public MissionData merchantShipMission;

    public MissionData navyPatrolMission;

    private int missionCrewCount;

    private bool missionFinished;

    private bool missionWorkInProgress;

    private VoyageData activeVoyage;

    private ShipState GetSelectedShip()
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogError(
                "ShipManager not found."
            );

            return null;
        }

        // If a ship was explicitly selected,
        // use that ship.
        if (
            !string.IsNullOrEmpty(
                SceneNavigator.selectedShipId
            )
        )
        {
            ShipState selectedShip =
                ShipManager.Instance.GetShip(
                    SceneNavigator.selectedShipId
                );

            if (
                selectedShip != null &&
                !selectedShip.onVoyage
            )
            {
                return selectedShip;
            }
        }

        // No ship was explicitly selected.
        // Find the first ship that is available.
        foreach (
            ShipState ship
            in ShipManager.Instance.ships
        )
        {
            if (
                ship != null &&
                !ship.onVoyage
            )
            {
                SceneNavigator.selectedShipId =
                    ship.shipId;

                Debug.Log(
                    "SHIP AUTO-SELECTED: " +
                    ship.shipName +
                    " | ID: " +
                    ship.shipId
                );

                return ship;
            }
        }

        Debug.LogError(
            "No available ship found."
        );

        return null;
    }

    public void SelectNearbyWreck()
    {
        Debug.Log(
            "WRECK SELECTED | Destination: " +
            nearbyWreckMission.destinationPoint
        );

        SelectMission(
            nearbyWreckMission,
            0
        );
    }

    public void SelectMerchantShip()
    {
        SelectMission(
            merchantShipMission,
            1
        );
    }

    public void SelectNavyPatrol()
    {
        SelectMission(
            navyPatrolMission,
            2
        );
    }

    private void RebindMissionPoints()
    {
        GameObject point1 =
            GameObject.Find("MissionPoint1");

        GameObject point2 =
            GameObject.Find("MissionPoint2");

        GameObject point3 =
            GameObject.Find("MissionPoint3");

        if (point1 != null)
        {
            nearbyWreckMission.destinationPoint =
                point1.transform;
        }

        if (point2 != null)
        {
            merchantShipMission.destinationPoint =
                point2.transform;
        }

        if (point3 != null)
        {
            navyPatrolMission.destinationPoint =
                point3.transform;
        }

        Debug.Log(
            "MISSION POINTS REBOUND | " +
            "Wreck: " +
            nearbyWreckMission.destinationPoint +
            " | Merchant: " +
            merchantShipMission.destinationPoint +
            " | Navy: " +
            navyPatrolMission.destinationPoint
        );
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        currentMission = null;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(
    Scene scene,
    LoadSceneMode mode
)
    {
        RebindMissionPoints();

        // Reconnect to the ShipMover belonging to the current scene.
        shipMover = FindFirstObjectByType<ShipMover>();

        if (shipMover != null)
        {
            Debug.Log(
                "MISSION MANAGER RECONNECTED SHIP MOVER: " +
                shipMover.name
            );
        }
        else
        {
            Debug.Log(
                "MISSION MANAGER: No ShipMover in this scene."
            );
        }
    }

    private void Update()
    {
        if (!missionWorkInProgress)
        {
            return;
        }

        if (activeVoyage == null)
        {
            return;
        }

        long now =
            System.DateTimeOffset.UtcNow
                .ToUnixTimeMilliseconds();

        long remaining =
            activeVoyage.missionCompletionTime -
            now;

        float remainingSeconds =
            Mathf.Max(
                0f,
                remaining / 1000f
            );

        missionTimer =
            remainingSeconds;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionTimer(
                remainingSeconds
            );
        }

        if (
            now >=
            activeVoyage.missionCompletionTime
        )
        {
            missionWorkInProgress = false;

            FinishMission();
        }
    }

    public void SelectMission(
    MissionData mission,
    int missionIndex
)
    {
        if (
            GameManager.Instance
            .mainBuildingLevel <
            mission.requiredMainBuildingLevel
        )
        {
            Debug.Log(
                "Mission Locked!"
            );

            UIManager.Instance
                .UpdateMissionStatus(
                    "REQUIRES MAIN BUILDING LEVEL " +
                    mission.requiredMainBuildingLevel
                );

            return;
        }

        currentMission = mission;

        UIManager.Instance
            .HighlightMission(
                missionIndex
            );

        UIManager.Instance
            .UpdateMissionUI();
    }

    public void AddCrew()
    {
        if (
            crewAssigned >=
            GameManager.Instance
            .availableCrew
        )
        {
            return;
        }

        crewAssigned++;

        UIManager.Instance
            .UpdateMissionUI();
    }

    public void RemoveCrew()
    {
        if (crewAssigned <= 0)
        {
            return;
        }

        crewAssigned--;

        UIManager.Instance
            .UpdateMissionUI();
    }

    public void StartMission()
    {
        // =========================================================
        // VALIDATION
        // =========================================================

        if (
            currentMission == null ||
            currentMission.destinationPoint == null
        )
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "SELECT A VALID MISSION"
                );

            return;
        }

        if (crewAssigned <= 0)
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "ASSIGN CREW"
                );

            return;
        }

        if (
            currentMission.requiredCrew > 0 &&
            crewAssigned < currentMission.requiredCrew
        )
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "REQUIRES " +
                    currentMission.requiredCrew +
                    " CREW"
                );

            return;
        }

        // =========================================================
        // SUPPLIES
        // =========================================================

        int supplyCost =
            crewAssigned * 2;

        if (
            GameManager.Instance.supplies <
            supplyCost
        )
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "NOT ENOUGH SUPPLIES"
                );

            return;
        }

        // =========================================================
        // PREVENT DUPLICATE MISSION
        // =========================================================

        if (missionInProgress)
        {
            return;
        }

        // =========================================================
        // GET SHIP
        // =========================================================

        ShipState ship =
            GetSelectedShip();

        if (ship == null)
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "NO AVAILABLE SHIP"
                );

            Debug.LogError(
                "START MISSION FAILED | " +
                "No available ship."
            );

            return;
        }

        // =========================================================
        // FIND EXACT CREW FOR THIS VOYAGE
        // =========================================================

        List<CrewMovement> selectedCrew =
            new List<CrewMovement>();

        foreach (
            CrewMovement crew
            in CrewManager.Instance.crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }

            if (crew.crewData == null)
            {
                continue;
            }

            if (
                crew.currentJob !=
                CrewMovement.CrewJob.Idle
            )
            {
                continue;
            }

            if (crew.assignedToMission)
            {
                continue;
            }

            selectedCrew.Add(
                crew
            );

            if (
                selectedCrew.Count >=
                crewAssigned
            )
            {
                break;
            }
        }

        // =========================================================
        // MAKE SURE WE ACTUALLY FOUND ENOUGH CREW
        // =========================================================

        if (
            selectedCrew.Count <
            crewAssigned
        )
        {
            Debug.LogError(
                "START MISSION FAILED | " +
                "Requested " +
                crewAssigned +
                " crew but only found " +
                selectedCrew.Count
            );

            UIManager.Instance
                .UpdateMissionStatus(
                    "NOT ENOUGH AVAILABLE CREW"
                );

            return;
        }

        // =========================================================
        // REMOVE ISLAND SUPPLIES
        // =========================================================

        GameManager.Instance.supplies -=
            supplyCost;

        // =========================================================
        // CREATE VOYAGE
        // =========================================================

        activeVoyage =
            new VoyageData();

        activeVoyage.shipId =
            ship.shipId;

        activeVoyage.voyageName =
            currentMission.missionName;

        activeVoyage.missionData =
            currentMission;

        activeVoyage.missionDuration =
            Mathf.Max(
                0f,
                currentMission.duration
            );

        activeVoyage.missionStartTime =
            0;

        activeVoyage.missionCompletionTime =
            0;

        activeVoyage.outcomeGenerated =
            false;

        activeVoyage.missionSucceeded =
            false;

        activeVoyage.progress =
            0f;

        activeVoyage.currentWaypointIndex =
            0;

        activeVoyage.voyagePhase =
            VoyagePhase.LeavingIsland;

        activeVoyage.supplies =
            supplyCost;

        activeVoyage.cargoCapacity =
            20 +
            (
                (GameManager.Instance.cargoLevel - 1)
                * 10
            );

        activeVoyage.cargo =
            new List<CargoStack>();

        activeVoyage.crew =
            new List<VoyageCrewData>();

        // =========================================================
        // BUILD CREW MANIFEST
        // =========================================================

        int manifestIndex = 0;

        foreach (
            CrewMovement crew
            in selectedCrew
        )
        {
            if (
                crew == null ||
                crew.crewData == null
            )
            {
                continue;
            }

            // -----------------------------------------------------
            // ASSIGN SHIP ROLE
            // -----------------------------------------------------

            VoyageRole role;

            if (manifestIndex == 0)
            {
                role = VoyageRole.Captain;
            }
            else
            {
                role = VoyageRole.Sailor;
            }

            // -----------------------------------------------------
            // CREATE VOYAGE CREW RECORD
            // -----------------------------------------------------

            VoyageCrewData voyageCrew =
                new VoyageCrewData();

            voyageCrew.crewId =
                crew.crewData.crewId;

            voyageCrew.crewName =
                crew.crewData.crewName;

            voyageCrew.shipRole =
                role;

            activeVoyage.crew.Add(
                voyageCrew
            );

            // -----------------------------------------------------
            // UPDATE PERSISTENT CREW IDENTITY
            // -----------------------------------------------------

            CrewManager.Instance.SetVoyageState(
                crew.crewData.crewId,
                ship.shipId,
                role
            );

            // -----------------------------------------------------
            // MARK PHYSICAL CREW
            // -----------------------------------------------------

            crew.assignedToMission =
                true;

            crew.currentJob =
                CrewMovement.CrewJob.Mission;

            manifestIndex++;

            Debug.Log(
                "VOYAGE CREW ADDED | " +
                crew.crewData.crewName +
                " | ID: " +
                crew.crewData.crewId +
                " | Role: " +
                role
            );
        }

        // =========================================================
        // CREW COUNT
        // =========================================================

        activeVoyage.crewCount =
            activeVoyage.crew.Count;

        // =========================================================
        // UPDATE SHIP STATE
        // =========================================================

        ship.onVoyage =
            true;

        ship.supplies =
            supplyCost;

        ship.sailLevel =
            GameManager.Instance.sailLevel;

        ship.cargoLevel =
            GameManager.Instance.cargoLevel;

        ship.cannonLevel =
            GameManager.Instance.cannonLevel;

        ship.crewCount =
            activeVoyage.crewCount;

        ship.destinationName =
            currentMission.missionName;

        ship.voyageProgress =
            0f;

        ship.voyagePhase =
            VoyagePhase.LeavingIsland;

        // =========================================================
        // SELECT THIS SHIP
        // =========================================================

        SceneNavigator.selectedShipId =
            ship.shipId;

        Debug.Log(
            "SELECTED SHIP SET: " +
            SceneNavigator.selectedShipId
        );

        // =========================================================
        // DEBUG VOYAGE MANIFEST
        // =========================================================

        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "VOYAGE CREATED"
        );

        Debug.Log(
            "Voyage: " +
            activeVoyage.voyageName
        );

        Debug.Log(
            "Ship: " +
            ship.shipName +
            " | ID: " +
            ship.shipId
        );

        Debug.Log(
            "Crew Count: " +
            activeVoyage.crewCount
        );

        foreach (
            VoyageCrewData crew
            in activeVoyage.crew
        )
        {
            Debug.Log(
                "MANIFEST | " +
                crew.crewName +
                " | ID: " +
                crew.crewId +
                " | Role: " +
                crew.shipRole
            );
        }

        Debug.Log(
            "Supplies: " +
            activeVoyage.supplies
        );

        Debug.Log(
            "Cargo Capacity: " +
            activeVoyage.cargoCapacity
        );

        Debug.Log(
            "========================================"
        );

        // =========================================================
        // ADD VOYAGE TO VOYAGE MANAGER
        // =========================================================

        if (
            VoyageManager.Instance == null
        )
        {
            Debug.LogError(
                "VoyageManager.Instance is missing."
            );

            return;
        }

        VoyageManager.Instance
            .activeVoyages
            .Add(
                activeVoyage
            );

        // =========================================================
        // MISSION STATE
        // =========================================================

        missionInProgress =
            true;

        missionFinished =
            false;

        missionWorkInProgress =
            false;

        missionCrewCount =
            activeVoyage.crewCount;

        // =========================================================
        // SEND EXACT CREW TO DOCK
        // =========================================================

        UIManager.Instance
            .UpdateMissionStatus(
                "CREW BOARDING..."
            );

        GameManager.Instance.availableCrew -=
            activeVoyage.crewCount;

        CrewManager.Instance
            .SendCrewToMission(
                activeVoyage.crewCount
            );

        // =========================================================
        // UPDATE UI
        // =========================================================

        UIManager.Instance
            .UpdateMissionUI();

        Debug.Log(
            "MISSION STARTED | " +
            currentMission.missionName +
            " | Crew: " +
            activeVoyage.crewCount
        );
    }

    public void BeginShipDeparture()
    {
        Debug.Log(
            "BeginShipDeparture CALLED"
        );

        // Make sure we have the ShipMover from the current scene.
        if (shipMover == null)
        {
            shipMover =
                FindFirstObjectByType<ShipMover>();

            if (shipMover == null)
            {
                Debug.LogError(
                    "Cannot start voyage: ShipMover not found."
                );

                return;
            }

            Debug.Log(
                "SHIP MOVER RECONNECTED BEFORE DEPARTURE"
            );
        }

        if (currentMission == null)
        {
            Debug.Log(
                "No mission selected!"
            );

            return;
        }

        missionFinished = false;

        missionWorkInProgress = false;

        shipMover.StartJourney();

        UIManager.Instance
            .UpdateMissionStatus(
                "SAILING TO " +
                currentMission.missionName.ToUpper()
            );

        Debug.Log(
            "Voyage Started: " +
            currentMission.missionName
        );
    }

    public void BeginMissionWork()
    {
        if (
            !missionInProgress ||
            currentMission == null ||
            activeVoyage == null
        )
        {
            return;
        }

        // =========================================================
        // START PERSISTENT MISSION TIMELINE
        // =========================================================

        activeVoyage.missionDuration =
            Mathf.Max(
                0f,
                currentMission.duration
            );

        long now =
            System.DateTimeOffset.UtcNow
                .ToUnixTimeMilliseconds();

        activeVoyage.missionStartTime = now;

        activeVoyage.missionCompletionTime =
            now +
            (long)(activeVoyage.missionDuration * 1000f);

        activeVoyage.outcomeGenerated = false;
        activeVoyage.missionSucceeded = false;

        missionWorkInProgress = true;

        // =========================================================
        // CHANGE VOYAGE PHASE
        // =========================================================

        VoyageManager.Instance.SetVoyagePhase(
            activeVoyage,
            VoyagePhase.Mission
        );

        UIManager.Instance.UpdateMissionStatus(
            "EXPLORING " +
            currentMission.missionName.ToUpper()
        );

        Debug.Log(
            "MISSION WORK STARTED: " +
            currentMission.missionName
        );

        Debug.Log(
            "MISSION DURATION: " +
            activeVoyage.missionDuration +
            " seconds"
        );

        Debug.Log(
            "MISSION START TIME: " +
            activeVoyage.missionStartTime
        );

        Debug.Log(
            "MISSION COMPLETION TIME: " +
            activeVoyage.missionCompletionTime
        );
    }

    // =========================================================
    // ACTIVE VOYAGE
    // =========================================================

    public VoyageData GetActiveVoyage()
    {
        /*
         * Return the currently active voyage.
         *
         * The voyage is created when the player starts
         * a mission and remains the single source of truth
         * for the crew manifest.
         */

        if (activeVoyage == null)
        {
            Debug.LogWarning(
                "MissionManager: No active voyage exists."
            );

            return null;
        }

        return activeVoyage;
    }

    public void FinishMission()
    {
        if (missionFinished)
        {
            return;
        }

        missionFinished = true;

        int successChance =
            GetSuccessChance();

        int randomRoll =
            Random.Range(
                0,
                100
            );

        if (
            randomRoll <
            successChance
        )
        {
            int reward =
                CalculateMissionReward(
                    currentMission.reward
                );

            GameManager.Instance
                .AddGold(
                    reward
                );

            GameManager.Instance
                .ChangeMorale(
                    5
                );

            UIManager.Instance
                .UpdateMissionStatus(
                    "MISSION SUCCESS\n+" +
                    reward +
                    " GOLD\n+5 MORALE"
                );

            Debug.Log(
                "Mission Successful!"
            );
        }
        else
        {
            GameManager.Instance
                .ChangeMorale(
                    -10
                );

            UIManager.Instance
                .UpdateMissionStatus(
                    "MISSION FAILED\n-10 MORALE"
                );

            Debug.Log(
                "Mission Failed!"
            );
        }


        /*
         * IMPORTANT:
         *
         * Do NOT return the crew yet.
         * Do NOT remove the voyage yet.
         * Do NOT unload supplies yet.
         *
         * The ship still has to travel home.
         */

        if (
            VoyageManager.Instance != null &&
            activeVoyage != null
        )
        {
            VoyageManager.Instance.SetVoyagePhase(
                activeVoyage,
                VoyagePhase.ReturningHome
            );
        }


        UIManager.Instance
            .UpdateMissionStatus(
                "MISSION COMPLETE\n" +
                "RETURNING TO ISLAND..."
            );


        Debug.Log(
            "MISSION COMPLETE - " +
            "SHIP RETURNING HOME"
        );
    }

    public void CompleteVoyageReturn()
    {
        if (
            activeVoyage == null
        )
        {
            Debug.LogWarning(
                "No active voyage to complete."
            );

            return;
        }

        Debug.Log(
            "SHIP ARRIVED BACK AT ISLAND"
        );


        /*
         * Return the physical crew.
         */

        CrewManager.Instance
            .ReturnCrew();


        /*
         * Return the crew count to the island.
         */

        GameManager.Instance
            .availableCrew +=
            missionCrewCount;


        missionCrewCount = 0;

        crewAssigned = 0;


        /*
         * Unload voyage supplies.
         */

        int returnedSupplies =
            activeVoyage.supplies;


        GameManager.Instance
            .supplies +=
            returnedSupplies;


        GameManager.Instance
            .supplies =
            Mathf.Clamp(
                GameManager.Instance.supplies,
                0,
                GameManager.Instance.maxSupplies
            );


        Debug.Log(
            "VOYAGE RETURNED | " +
            "Supplies unloaded: " +
            returnedSupplies +
            " | Island supplies: " +
            GameManager.Instance.supplies
        );


        /*
         * Mark the ship as home.
         */

        ShipState ship =
            ShipManager.Instance.GetShip(
                activeVoyage.shipId
            );


        if (
            ship != null
        )
        {
            ship.onVoyage = false;

            ship.voyageProgress = 100f;

            ship.voyagePhase =
                VoyagePhase.Complete;

            ship.destinationName = "";
        }


        /*
         * Remove voyage from active voyages.
         */

        VoyageManager.Instance
            .activeVoyages
            .Remove(
                activeVoyage
            );


        activeVoyage = null;

        missionInProgress = false;

        missionWorkInProgress = false;

        currentMission = null;

        missionFinished = false;


        UIManager.Instance
            .UpdateMissionUI();


        UIManager.Instance
            .UpdateMissionStatus(
                "VOYAGE COMPLETE"
            );


        Debug.Log(
            "VOYAGE COMPLETE - " +
            "SHIP IS HOME"
        );

        ShipMover shipMover =
    FindFirstObjectByType<ShipMover>();

        if (shipMover != null)
        {
            shipMover.RestoreShipAtDock();

            Debug.Log(
                "ISLAND SHIP RESTORED AT DOCK"
            );
        }
        else
        {
            Debug.LogWarning(
                "Could not find ShipMover to restore the ship."
            );
        }

        if (SceneNavigator.Instance != null)
        {
            Debug.Log(
                "VOYAGE COMPLETE - RETURNING TO ISLAND SCENE"
            );

            SceneNavigator.Instance.GoToIsland();
        }
        else
        {
            Debug.LogError(
                "Cannot return to IslandScene. " +
                "SceneNavigator is missing."
            );
        }
    }

    public int GetSuccessChance()
    {
        int moraleModifier =
            Mathf.RoundToInt(
                (GameManager.Instance.morale - 50) / 5f
            );

        return Mathf.Clamp(
            (crewAssigned * 20)
            +
            (GameManager.Instance.cannonLevel * 5)
            +
            moraleModifier,
            0,
            100
        );
    }

    public int CalculateMissionReward(int baseReward)
    {
        float cargoMultiplier =
            1f +
            ((GameManager.Instance.cargoLevel - 1) * 0.25f);

        return Mathf.RoundToInt(
            baseReward * cargoMultiplier
        );
    }

    public void UpgradeSails()
    {
        if (
            GameManager.Instance.gold <
            GameManager.Instance
            .sailUpgradeCost
        )
        {
            Debug.Log(
                "Not enough gold!"
            );

            return;
        }

        GameManager.Instance.gold -=
            GameManager.Instance
            .sailUpgradeCost;

        GameManager.Instance
            .sailLevel++;

        shipMover.moveSpeed += 1f;

        GameManager.Instance
            .sailUpgradeCost += 150;

        Debug.Log(
            "Sails Upgraded!"
        );
    }

    public void UpgradeCargo()
    {
        if (
            GameManager.Instance.gold <
            GameManager.Instance
            .cargoUpgradeCost
        )
        {
            Debug.Log(
                "Not enough gold!"
            );

            return;
        }

        GameManager.Instance.gold -=
            GameManager.Instance
            .cargoUpgradeCost;

        GameManager.Instance
            .cargoLevel++;

        GameManager.Instance
            .cargoUpgradeCost += 200;

        Debug.Log(
            "Cargo Upgraded!"
        );
    }

    public void UpgradeCannons()
    {
        if (
            GameManager.Instance.gold <
            GameManager.Instance
            .cannonUpgradeCost
        )
        {
            Debug.Log(
                "Not enough gold!"
            );

            return;
        }

        GameManager.Instance.gold -=
            GameManager.Instance
            .cannonUpgradeCost;

        GameManager.Instance
            .cannonLevel++;

        GameManager.Instance
            .cannonUpgradeCost += 250;

        Debug.Log(
            "Cannons Upgraded!"
        );
    }

    void ResetMissionStatus()
    {
        UIManager.Instance
            .UpdateMissionStatus(
                "READY"
            );
    }
}
