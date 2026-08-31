using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    // =========================================================
    // ISLAND
    // =========================================================

    [Header("Island")]

    public int mainBuildingLevel = 1;


    // =========================================================
    // RESOURCES
    // =========================================================

    [Header("Resources")]

    public int gold = 1000;

    public int goldStorage = 1000;


    // =========================================================
    // SUPPLIES
    // =========================================================

    [Header("Supplies")]

    public int supplies = 50;

    public int maxSupplies = 1000;

    private float fishingTimer;


    // =========================================================
    // CREW
    // =========================================================

    [Header("Crew")]

    public int availableCrew = 5;

    public int maxCrew = 5;


    // =========================================================
    // MORALE
    // =========================================================

    [Header("Morale")]

    public int morale = 50;


    // =========================================================
    // SHIP UPGRADES
    // =========================================================

    [Header("Ship Upgrades")]

    public int sailLevel = 1;

    public int cargoLevel = 1;

    public int cannonLevel = 1;


    [Header("Ship Upgrade Costs")]

    public int sailUpgradeCost = 200;

    public int cargoUpgradeCost = 300;

    public int cannonUpgradeCost = 400;


    // =========================================================
    // BUILDING UNLOCKS
    // =========================================================

    [Header("Building Unlocks")]

    public bool liveMusicUnlocked;

    public bool expandedWarehouseUnlocked;

    public bool veteranCrewUnlocked;

    public bool tradeRoutesUnlocked;


    // =========================================================
    // BUILDING STATE
    // =========================================================

    [Header("Persistent Building State")]

    public BuildingPersistentState[] buildings;


    // =========================================================
    // AWAKE
    // =========================================================

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

        InitializeBuildingState();

        Debug.Log(
            "GAME MANAGER READY"
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleFishing();

        HandleBuildingProgress();
    }


    // =========================================================
    // INITIALIZE BUILDINGS
    // =========================================================

    private void InitializeBuildingState()
    {
        if (
            buildings != null &&
            buildings.Length > 0
        )
        {
            return;
        }

        buildings =
            new BuildingPersistentState[]
            {
                new BuildingPersistentState(
                    "Main Building",
                    1
                ),

                new BuildingPersistentState(
                    "Storage",
                    1
                ),

                new BuildingPersistentState(
                    "Tavern",
                    1
                ),

                new BuildingPersistentState(
                    "Barracks",
                    1
                ),

                new BuildingPersistentState(
                    "Docks",
                    1
                )
            };
    }


    // =========================================================
    // FIND BUILDING STATE
    // =========================================================

    public BuildingPersistentState GetBuildingState(
        string buildingName
    )
    {
        if (
            buildings == null
        )
        {
            return null;
        }

        foreach (
            BuildingPersistentState building
            in buildings
        )
        {
            if (
                building != null &&
                building.buildingName ==
                buildingName
            )
            {
                return building;
            }
        }

        return null;
    }


    // =========================================================
    // FISHING
    // =========================================================

    private void HandleFishing()
    {
        fishingTimer +=
            Time.deltaTime;

        if (
            CrewManager.Instance == null
        )
        {
            return;
        }

        if (
            fishingTimer < 5f
        )
        {
            return;
        }

        fishingTimer = 0f;

        int fishingCrew =
            CrewManager.Instance
                .fishingCrew;


        if (
            fishingCrew <= 0
        )
        {
            return;
        }


        int suppliesGained =
            fishingCrew;


        supplies +=
            suppliesGained;


        supplies =
            Mathf.Clamp(
                supplies,
                0,
                maxSupplies
            );


        Debug.Log(
            "FISHING | Crew: " +
            fishingCrew +
            " | Supplies +" +
            suppliesGained
        );
    }


    // =========================================================
    // BUILDING PROGRESS
    // =========================================================

    private void HandleBuildingProgress()
    {
        if (
            buildings == null
        )
        {
            return;
        }

        foreach (
            BuildingPersistentState building
            in buildings
        )
        {
            if (
                building == null
            )
            {
                continue;
            }

            if (
                !building.upgrading
            )
            {
                continue;
            }

            if (
                !building.constructionStarted
            )
            {
                continue;
            }


            float buildSpeed = 1f;


            if (
                CrewManager.Instance != null
            )
            {
                buildSpeed =
                    1f +
                    Mathf.Sqrt(
                        CrewManager.Instance
                            .builderCrew
                    );
            }


            building.currentUpgradeTimer +=
                Time.deltaTime *
                buildSpeed;


            if (
                building.currentUpgradeTimer >=
                building.upgradeTime
            )
            {
                FinishBuildingUpgrade(
                    building
                );
            }
        }
    }


    // =========================================================
    // START BUILDING
    // =========================================================

    public bool StartBuildingUpgrade(
        string buildingName,
        int cost,
        float upgradeTime
    )
    {
        BuildingPersistentState building =
            GetBuildingState(
                buildingName
            );


        if (
            building == null
        )
        {
            Debug.LogError(
                "BUILDING STATE NOT FOUND | " +
                buildingName
            );

            return false;
        }


        if (
            building.upgrading
        )
        {
            return false;
        }


        if (
            !SpendGold(cost)
        )
        {
            return false;
        }


        building.upgrading =
            true;

        building.constructionStarted =
            false;

        building.currentUpgradeTimer =
            0f;

        building.upgradeTime =
            upgradeTime;

        building.upgradeCost =
            cost;


        Debug.Log(
            "BUILDING UPGRADE STARTED | " +
            buildingName
        );


        return true;
    }


    // =========================================================
    // CONSTRUCTION STARTED
    // =========================================================

    public void ConstructionStarted(
        string buildingName
    )
    {
        BuildingPersistentState building =
            GetBuildingState(
                buildingName
            );


        if (
            building == null
        )
        {
            return;
        }


        building.constructionStarted =
            true;


        Debug.Log(
            "CONSTRUCTION STARTED | " +
            buildingName
        );
    }


    // =========================================================
    // FINISH BUILDING
    // =========================================================

    private void FinishBuildingUpgrade(
        BuildingPersistentState building
    )
    {
        building.upgrading =
            false;

        building.constructionStarted =
            false;

        building.currentUpgradeTimer =
            0f;


        building.buildingLevel++;


        Debug.Log(
            "BUILDING COMPLETE | " +
            building.buildingName +
            " | LEVEL " +
            building.buildingLevel
        );


        ApplyBuildingUpgrade(
            building.buildingName
        );


        if (
            CrewManager.Instance != null
        )
        {
            CrewManager.Instance
                .FinishConstructionCrew(
                    building.buildingName
                );
        }
    }


    // =========================================================
    // APPLY BUILDING BENEFITS
    // =========================================================

    private void ApplyBuildingUpgrade(
        string buildingName
    )
    {
        switch (
            buildingName
        )
        {
            case "Storage":

                goldStorage += 500;

                maxSupplies += 25;

                break;


            case "Tavern":

                morale =
                    Mathf.Clamp(
                        morale + 10,
                        0,
                        100
                    );

                break;


            case "Barracks":

                maxCrew += 2;

                break;


            case "Main Building":

                mainBuildingLevel++;

                break;
        }
    }


    // =========================================================
    // GOLD
    // =========================================================

    public void AddGold(
        int amount
    )
    {
        gold += amount;

        gold =
            Mathf.Clamp(
                gold,
                0,
                goldStorage
            );
    }


    public bool SpendGold(
        int amount
    )
    {
        if (
            gold < amount
        )
        {
            return false;
        }

        gold -= amount;

        return true;
    }


    // =========================================================
    // MORALE
    // =========================================================

    public void ChangeMorale(
        int amount
    )
    {
        morale =
            Mathf.Clamp(
                morale + amount,
                0,
                100
            );
    }
}


// =============================================================
// PERSISTENT BUILDING STATE
// =============================================================

[System.Serializable]
public class BuildingPersistentState
{
    public string buildingName;

    public int buildingLevel;

    public bool upgrading;

    public bool constructionStarted;

    public float currentUpgradeTimer;

    public float upgradeTime;

    public int upgradeCost;


    public BuildingPersistentState(
        string name,
        int level
    )
    {
        buildingName =
            name;

        buildingLevel =
            level;

        upgrading =
            false;

        constructionStarted =
            false;

        currentUpgradeTimer =
            0f;

        upgradeTime =
            20f;

        upgradeCost =
            100;
    }


    public float GetProgress()
    {
        if (
            upgradeTime <= 0f
        )
        {
            return 0f;
        }

        return Mathf.Clamp01(
            currentUpgradeTimer /
            upgradeTime
        );
    }
}