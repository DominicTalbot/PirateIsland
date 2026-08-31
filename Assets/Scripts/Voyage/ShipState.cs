using System;
using System.Collections.Generic;

[Serializable]
public class ShipState
{
    public string shipId;

    public string shipName;

    public float worldX;
    public float worldZ;

    // =========================================================
    // VOYAGE
    // =========================================================

    public bool onVoyage;

    public float voyageProgress;

    public VoyagePhase voyagePhase =
        VoyagePhase.LeavingIsland;

    public int currentWaypointIndex;

    public string destinationName;

    // =========================================================
    // VOYAGE TIMELINES
    // =========================================================

    public long outboundStartTime;

    public long outboundCompletionTime;

    public long missionStartTime;

    public long missionCompletionTime;

    public long returnStartTime;

    public long returnCompletionTime;

    public long approachStartTime;

    public long approachCompletionTime;

    public float missionDuration;

    public bool outcomeGenerated;

    public bool missionSucceeded;

    public bool needsAttention;

    // =========================================================
    // SHIP RESOURCES
    // =========================================================

    public int supplies;

    public int cargoCapacity = 20;

    public List<CargoStack> cargo =
        new List<CargoStack>();

    public float morale = 100f;

    public int crewCount;

    // =========================================================
    // UPGRADES
    // =========================================================

    public int sailLevel = 1;

    public int cargoLevel = 1;

    public int cannonLevel = 1;
}