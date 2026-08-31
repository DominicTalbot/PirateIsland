using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int saveVersion;

    public int gold;

    public int maxGold;

    public int crew;

    public int maxCrew;

    public int morale;

    public int supplies;

    public int maxSupplies;

    public int mainBuildingLevel;

    public int sailLevel;

    public int cargoLevel;

    public int cannonLevel;

    public int sailUpgradeCost;

    public int cargoUpgradeCost;

    public int cannonUpgradeCost;


    // =========================================================
    // BUILDINGS
    // =========================================================

    public List<BuildingSaveData>
        buildings =
            new List<BuildingSaveData>();


    // =========================================================
    // CREW
    // =========================================================

    public List<CrewData>
        crewData =
            new List<CrewData>();

    // =========================================================
    // SHIPS
    // =========================================================

    public List<ShipState> ships =
        new List<ShipState>();
}