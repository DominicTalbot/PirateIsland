using System;

[Serializable]
public class CrewData
{
    // =========================================================
    // PERMANENT IDENTITY
    // =========================================================

    public string crewId;
    public string crewName;


    // =========================================================
    // ISLAND STATE
    // =========================================================

    public CrewIslandJob islandJob;

    /*
     * Persistent fishing spot assignment.
     *
     * We store the spot index rather than a Transform because
     * Transforms belong to the Island scene and are destroyed
     * when the scene changes.
     */
    public int fishingSpotIndex = -1;


    /*
     * Persistent building assignment.
     *
     * We store the building name rather than a BuildingClickable
     * reference because the BuildingClickable belongs to the
     * Island scene.
     */
    public string assignedBuildingName = "";


    /*
     * Whether the crew member had actually reached their
     * work position.
     *
     * This allows us to restore the difference between:
     *
     * Walking to work
     *
     * and
     *
     * Already working.
     */
    public bool wasWorking;


    // =========================================================
    // VOYAGE STATE
    // =========================================================

    public bool isOnVoyage;

    public string assignedShipId;

    public VoyageRole shipRole;

    public CrewIslandJob previousIslandJob =
    CrewIslandJob.Idle;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public CrewData(
        string id,
        string name
    )
    {
        crewId = id;
        crewName = name;

        islandJob =
            CrewIslandJob.Idle;

        fishingSpotIndex =
            -1;

        assignedBuildingName =
            "";

        wasWorking =
            false;

        isOnVoyage =
            false;

        assignedShipId =
            "";

        shipRole =
            VoyageRole.Sailor;
    }
}