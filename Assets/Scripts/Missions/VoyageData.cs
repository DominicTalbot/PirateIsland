using System;
using System.Collections.Generic;

[Serializable]
public class VoyageCrewData
{
    public string crewId;
    public string crewName;
    public VoyageRole shipRole;
}

[Serializable]
public class VoyageData
{
    // =========================================================
    // VOYAGE IDENTITY
    // =========================================================

    public string shipId;
    public string voyageName;

    public MissionData missionData;


    // =========================================================
    // CREW
    // =========================================================

    public int crewCount;

    public List<VoyageCrewData> crew =
        new List<VoyageCrewData>();


    // =========================================================
    // VOYAGE PROGRESS
    // =========================================================

    public float progress;

    public int currentWaypointIndex;

    public VoyagePhase voyagePhase =
        VoyagePhase.LeavingIsland;


    // =========================================================
    // VOYAGE RESOURCES
    // =========================================================

    public int supplies;

    public float supplyTimer;

    public float morale = 100f;

    public int cargoCapacity;

    public List<CargoStack> cargo =
        new List<CargoStack>();


    // =========================================================
    // OUTBOUND TRAVEL TIMELINE
    // =========================================================

    public long outboundStartTime;

    public long outboundCompletionTime;


    // =========================================================
    // MISSION TIMELINE
    // =========================================================

    public float missionDuration;

    public long missionStartTime;

    public long missionCompletionTime;


    // =========================================================
    // RETURN TRAVEL TIMELINE
    // =========================================================

    public long returnStartTime;

    public long returnCompletionTime;


    // =========================================================
    // APPROACH TIMELINE
    // =========================================================

    public long approachStartTime;

    public long approachCompletionTime;


    // =========================================================
    // MISSION RESULT
    // =========================================================

    public bool outcomeGenerated;

    public bool missionSucceeded;


    // =========================================================
    // VOYAGE EVENTS
    // =========================================================

    public bool needsAttention;
}