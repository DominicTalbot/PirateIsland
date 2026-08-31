using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    private string savePath;

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

        savePath =
            Application.persistentDataPath +
            "/save.json";

        Debug.Log(
            "SAVE PATH: " +
            savePath
        );
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.saveVersion = 2;

        data.gold =
            GameManager.Instance.gold;

        data.maxGold =
            GameManager.Instance.goldStorage;

        data.crew =
            GameManager.Instance.availableCrew;

        data.maxCrew =
            GameManager.Instance.maxCrew;

        data.morale =
            GameManager.Instance.morale;

        data.supplies =
            GameManager.Instance.supplies;

        data.maxSupplies =
            GameManager.Instance.maxSupplies;

        data.mainBuildingLevel =
            GameManager.Instance.mainBuildingLevel;

        data.sailLevel =
            GameManager.Instance.sailLevel;

        data.cargoLevel =
            GameManager.Instance.cargoLevel;

        data.cannonLevel =
            GameManager.Instance.cannonLevel;

        data.sailUpgradeCost =
            GameManager.Instance.sailUpgradeCost;

        data.cargoUpgradeCost =
            GameManager.Instance.cargoUpgradeCost;

        data.cannonUpgradeCost =
            GameManager.Instance.cannonUpgradeCost;

        // =========================================================
        // SAVE CREW DATA
        // =========================================================

        if (
            CrewManager.Instance != null &&
            CrewManager.Instance.crewData != null
        )
        {
            data.crewData =
                CrewManager.Instance.crewData;

            Debug.Log(
                "CREW DATA SAVED | Count: " +
                data.crewData.Count
            );
        }

        // =========================================================
        // SAVE SHIP DATA
        // =========================================================

        if (
            ShipManager.Instance != null &&
            ShipManager.Instance.ships != null
        )
        {
            data.ships =
                ShipManager.Instance.ships;

            Debug.Log(
                "SHIP DATA SAVED | Count: " +
                data.ships.Count
            );
        }

        BuildingClickable[] buildings =
    FindObjectsByType<BuildingClickable>();

        foreach (
            BuildingClickable building
            in buildings
        )
        {
            BuildingSaveData buildingData =
                new BuildingSaveData();

            buildingData.buildingName =
                building.buildingName;

            buildingData.buildingLevel =
                building.buildingLevel;

            buildingData.upgradeCost =
                building.upgradeCost;

            data.buildings.Add(
                buildingData
            );
        }

        string json =
            JsonUtility.ToJson(
                data,
                true
            );

        File.WriteAllText(
            savePath,
            json
        );

        Debug.Log(
            "Game Saved!"
        );
    }

    public void LoadGame()
    {
        if (
            !File.Exists(savePath)
        )
        {
            Debug.Log(
                "No Save Found!"
            );

            return;
        }

        string json =
            File.ReadAllText(
                savePath
            );

        SaveData data =
            JsonUtility.FromJson<SaveData>(
                json
            );

        // =========================================================
        // RESTORE CREW DATA
        // =========================================================

        if (
            data.crewData != null &&
            data.crewData.Count > 0
        )
        {
            if (CrewManager.Instance != null)
            {
                CrewManager.Instance.LoadCrewData(
                    data.crewData
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "NO CREW DATA FOUND IN SAVE FILE"
            );
        }

        // =========================================================
        // RESTORE SHIP DATA
        // =========================================================

        if (
            data.ships != null &&
            data.ships.Count > 0
        )
        {
            if (ShipManager.Instance != null)
            {
                ShipManager.Instance.ships =
                    data.ships;

                Debug.Log(
                    "SHIP DATA LOADED | Count: " +
                    data.ships.Count
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "NO SHIP DATA FOUND IN SAVE FILE"
            );
        }

        GameManager.Instance.gold =
            data.gold;

        GameManager.Instance.goldStorage =
            data.maxGold;

        GameManager.Instance.availableCrew =
            data.crew;

        GameManager.Instance.maxCrew =
            data.maxCrew;

        GameManager.Instance.morale =
            data.morale;

        if (
            data.saveVersion >= 2
        )
        {
            GameManager.Instance.supplies =
                data.supplies;

            GameManager.Instance.maxSupplies =
                data.maxSupplies;

            GameManager.Instance.mainBuildingLevel =
                data.mainBuildingLevel;

            GameManager.Instance.sailLevel =
                data.sailLevel;

            GameManager.Instance.cargoLevel =
                data.cargoLevel;

            GameManager.Instance.cannonLevel =
                data.cannonLevel;

            GameManager.Instance.sailUpgradeCost =
                data.sailUpgradeCost;

            GameManager.Instance.cargoUpgradeCost =
                data.cargoUpgradeCost;

            GameManager.Instance.cannonUpgradeCost =
                data.cannonUpgradeCost;
        }

        RestoreBuildingsFromData(
            data
        );

        Debug.Log(
            "Game Loaded!"
        );
    }

    public void RestoreBuildingState()
    {
        if (
            !File.Exists(savePath)
        )
        {
            Debug.Log(
                "No save file found. " +
                "Using default building levels."
            );

            return;
        }

        string json =
            File.ReadAllText(
                savePath
            );

        SaveData data =
            JsonUtility.FromJson<SaveData>(
                json
            );

        RestoreBuildingsFromData(
            data
        );

        Debug.Log(
            "BUILDING STATE RESTORED"
        );
    }

    private void RestoreBuildingsFromData(
        SaveData data
    )
    {
        if (
            data == null ||
            data.buildings == null
        )
        {
            Debug.LogWarning(
                "No building data found."
            );

            return;
        }

        BuildingClickable[] buildings =
    FindObjectsByType<BuildingClickable>();

        foreach (
            BuildingClickable building
            in buildings
        )
        {
            foreach (
                BuildingSaveData buildingData
                in data.buildings
            )
            {
                if (
                    building.buildingName ==
                    buildingData.buildingName
                )
                {
                    building.buildingLevel =
                        buildingData.buildingLevel;

                    building.upgradeCost =
                        buildingData.upgradeCost;

                    building.RefreshVisuals();

                    Debug.Log(
                        "BUILDING RESTORED | " +
                        building.buildingName +
                        " | Level: " +
                        building.buildingLevel
                    );

                    break;
                }
            }
        }
    }
}