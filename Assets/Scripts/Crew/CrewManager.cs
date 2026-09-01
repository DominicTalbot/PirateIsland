using System.Collections.Generic;
using UnityEngine;
using static CrewMovement;

public class CrewManager : MonoBehaviour
{
    public static CrewManager Instance;


    // =========================================================
    // CREW
    // =========================================================

    public List<CrewMovement> crewMembers =
        new List<CrewMovement>();


    [Header("Crew Data")]

    public List<CrewData> crewData =
        new List<CrewData>();


    // =========================================================
    // MISSION
    // =========================================================

    public Transform dockPoint;

    private int crewArrived;

    private int crewRequired;

    // =========================================================
    // VOYAGE
    // =========================================================

    public VoyageData activeVoyage;


    // =========================================================
    // JOBS
    // =========================================================

    [Header("Jobs")]

    public int fishingCrew;

    public int builderCrew;

    public int fishingAssigned;

    public int builderAssigned;


    // =========================================================
    // BUILDERS
    // =========================================================

    [Header("Builders")]

    public BuildingClickable activeConstruction;


    // =========================================================
    // FISHING
    // =========================================================

    [Header("Fishing")]

    public Transform[] fishingSpots;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CleanupCrewReferences();
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded +=
    OnSceneLoaded;

        if (crewData == null)
        {
            crewData = new List<CrewData>();
        }

        // InitializeCrewData();  // <-- replaced with delayed call
        StartCoroutine(InitializeCrewDataDelayed());

        Debug.Log(
            "CREW MANAGER READY | Crew Data: " +
            crewData.Count
        );
    }

    private System.Collections.IEnumerator InitializeCrewDataDelayed()
    {
        // Wait a frame so scene objects have run Awake() and had a chance
        // to register themselves with CrewManager.
        yield return null;
        // One more frame if you want to be extra-safe:
        yield return null;

        // Now initialize using any crewMembers already registered.
        InitializeCrewData();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        PrintCrewIdentity();
    }


    // =========================================================
    // INITIALIZE CREW DATA
    // =========================================================

    private void InitializeCrewData()
    {
        if (crewMembers == null)
        {
            crewMembers = new List<CrewMovement>();
        }

        if (crewData == null)
        {
            crewData = new List<CrewData>();
        }

        for (int i = 0; i < crewMembers.Count; i++)
        {
            CrewMovement crewObj = crewMembers[i];

            if (crewObj == null)
            {
                continue;
            }

            // If the runtime object already has authoritative CrewData, try to reconnect by id.
            if (crewObj.crewData != null)
            {
                CrewData existingById = GetCrewDataById(crewObj.crewData.crewId);
                if (existingById != null)
                {
                    crewObj.crewData = existingById;
                    Debug.Log("CREW DATA RECONNECTED (already had data) | " + existingById.crewName + " | ID: " + existingById.crewId);
                    continue;
                }
            }

            // Try deterministic reconnect by starterCrewId set on the prefab/instance.
            string prefabId = null;
            try
            {
                // CrewMovement exposes starterCrewId; read it directly.
                prefabId = crewObj.GetType().GetField("starterCrewId")?.GetValue(crewObj) as string;
            }
            catch
            {
                prefabId = null;
            }

            if (!string.IsNullOrWhiteSpace(prefabId))
            {
                CrewData byPrefabId = GetCrewDataById(prefabId);

                if (byPrefabId != null)
                {
                    // Reuse existing authoritative data.
                    crewObj.crewData = byPrefabId;
                    Debug.Log("CREW DATA RECONNECTED BY STARTER ID | " + byPrefabId.crewName + " | ID: " + byPrefabId.crewId);
                    continue;
                }
                else
                {
                    // Create a new CrewData using the explicit starter id (keeps IDs stable).
                    string newName = GenerateCrewName(i + 1);

                    CrewData created =
                        new CrewData(
                            prefabId,
                            newName
                        );

                    crewObj.crewData = created;
                    crewData.Add(created);

                    Debug.Log("CREW DATA CREATED FROM STARTER ID | " + newName + " | ID: " + prefabId);
                    continue;
                }
            }

            // Fallback: reconnect by any existing crewData in scene (if previous runs created data).
            string crewId = null;
            if (crewObj.crewData != null)
            {
                crewId = crewObj.crewData.crewId;
            }

            if (!string.IsNullOrWhiteSpace(crewId))
            {
                CrewData existingData = GetCrewDataById(crewId);

                if (existingData != null)
                {
                    crewObj.crewData = existingData;

                    Debug.Log(
                        "CREW DATA RECONNECTED | " +
                        existingData.crewName +
                        " | ID: " +
                        existingData.crewId
                    );

                    continue;
                }
            }

            // =========================================================
            // NEW CREW (no starter id and no existing data)
            // =========================================================
            string newCrewId = GenerateCrewId();

            string newCrewName = GenerateCrewName(i + 1);

            CrewData newData =
                new CrewData(
                    newCrewId,
                    newCrewName
                );

            crewObj.crewData = newData;

            crewData.Add(newData);

            Debug.Log(
                "NEW CREW DATA CREATED | " +
                newCrewName +
                " | ID: " +
                newCrewId
            );
        }
    }


    // =========================================================
    // GENERATE CREW ID
    // =========================================================

    private string GenerateCrewId()
    {
        int number = 1;

        while (true)
        {
            string id =
                "crew_" +
                number.ToString("000");

            bool exists = false;


            foreach (
                CrewData data
                in crewData
            )
            {
                if (
                    data != null &&
                    data.crewId == id
                )
                {
                    exists = true;
                    break;
                }
            }


            if (!exists)
            {
                return id;
            }


            number++;
        }
    }


    // =========================================================
    // GENERATE CREW NAME
    // =========================================================

    private string GenerateCrewName(
        int number
    )
    {
        return "Crew " + number;
    }


    // =========================================================
    // REGISTER CREW
    // =========================================================

    public bool RegisterCrew(
    CrewMovement crew
)
    {
        if (crew == null)
        {
            Debug.LogWarning("RegisterCrew called with null crew reference.");
            return false;
        }

        // Basic registration diagnostic
        {
            int goId = crew.gameObject.GetInstanceID();
            string goName = crew.gameObject.name;
            string hasData = crew.crewData != null ? "YES" : "NO";
            string starterId = "";
            // safe attempt to read starterCrewId if field exists on the instance
            try
            {
                var cm = crew.GetComponent<CrewMovement>();
                if (cm != null)
                {
                    // the companion field may be empty
                    var field = typeof(CrewMovement).GetField("starterCrewId");
                    if (field != null)
                    {
                        object val = field.GetValue(cm);
                        starterId = val != null ? val.ToString() : "";
                    }
                }
            }
            catch { }

            Debug.LogFormat(
                "RegisterCrew CALLED | GO: {0} (id:{1}) | HasCrewData: {2} | StarterId: '{3}' | crewMembersCount: {4}",
                goName,
                goId,
                hasData,
                starterId,
                crewMembers != null ? crewMembers.Count : 0
            );
        }

        // =========================================================
        // ALREADY REGISTERED
        // =========================================================
        if (crewMembers.Contains(crew))
        {
            Debug.Log("RegisterCrew: Already registered -> " + crew.name);
            return true;
        }

        // =========================================================
        // If the incoming runtime object doesn't have CrewData,
        // try deterministic reconnects before creating new data.
        // =========================================================
        if (crew.crewData == null)
        {
            // 1) Try explicit starterCrewId on the prefab/instance
            string starterId = "";
            try
            {
                var field = typeof(CrewMovement).GetField("starterCrewId");
                if (field != null)
                {
                    object val = field.GetValue(crew);
                    starterId = val != null ? val.ToString() : "";
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(starterId))
            {
                CrewData found = GetCrewDataById(starterId);
                if (found != null)
                {
                    crew.crewData = found;
                    Debug.LogFormat("RegisterCrew: Reconnected by starterId | GO: {0} | Id: {1} | Name: {2}", crew.name, found.crewId, found.crewName);
                }
                else
                {
                    Debug.LogFormat("RegisterCrew: starterId present but no matching CrewData found | GO: {0} | starterId: {1}", crew.name, starterId);
                }
            }

            // 2) Fallback: try matching by name against existing CrewData entries
            if (crew.crewData == null && crewData != null && crewData.Count > 0)
            {
                string sceneName = crew.gameObject.name;
                if (sceneName.EndsWith("(Clone)"))
                {
                    sceneName = sceneName.Replace("(Clone)", "").Trim();
                }

                foreach (CrewData d in crewData)
                {
                    if (d == null) continue;

                    if (string.Equals(d.crewName, sceneName, System.StringComparison.OrdinalIgnoreCase) ||
                        sceneName.IndexOf(d.crewName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // ensure no runtime object already represents this CrewData
                        CrewMovement existing = GetCrewById(d.crewId);
                        if (existing == null)
                        {
                            crew.crewData = d;
                            Debug.LogFormat("RegisterCrew: Reconnected by name | GO: {0} | MatchedData: {1} ({2})", crew.name, d.crewName, d.crewId);
                            break;
                        }
                    }
                }
            }
        }

        // =========================================================
        // CREW HAS DATA (either from prefab or reconnected above)
        // =========================================================
        if (crew.crewData != null)
        {
            string crewId = crew.crewData.crewId;

            // -----------------------------------------------------
            // FIND THE EXISTING RUNTIME CREW
            // -----------------------------------------------------
            CrewMovement existingCrew = null;

            for (int i = crewMembers.Count - 1; i >= 0; i--)
            {
                CrewMovement registeredCrew = crewMembers[i];

                // Remove destroyed/stale references.
                if (registeredCrew == null)
                {
                    crewMembers.RemoveAt(i);
                    continue;
                }

                if (registeredCrew.crewData == null)
                {
                    continue;
                }

                if (
                    registeredCrew.crewData.crewId ==
                    crewId
                )
                {
                    existingCrew = registeredCrew;
                    break;
                }
            }

            if (
                existingCrew != null &&
                existingCrew != crew
            )
            {
                Debug.LogFormat(
                    "RegisterCrew: DUPLICATE CREW OBJECT DETECTED | GO: {0} | ID: {1} | Existing: {2}",
                    crew.name,
                    crewId,
                    existingCrew.name
                );

                Destroy(crew.gameObject);

                return false;
            }

            // -----------------------------------------------------
            // RECONNECT TO AUTHORITATIVE DATA (ensure same data reference)
            // -----------------------------------------------------
            CrewData existingData = GetCrewDataById(crew.crewData.crewId);

            if (existingData != null)
            {
                crew.crewData = existingData;
            }
            else
            {
                crewData.Add(crew.crewData);
            }

            // -----------------------------------------------------
            // IF THIS CREW IS ON VOYAGE
            // -----------------------------------------------------
            if (crew.crewData.isOnVoyage)
            {
                crew.currentJob = CrewMovement.CrewJob.Voyage;
                crew.assignedToMission = true;
                crew.HideIslandRepresentation();

                Debug.LogFormat(
                    "RegisterCrew: Island representation hidden (on voyage) | GO: {0} | ID: {1}",
                    crew.name,
                    crew.crewData.crewId
                );
            }

            // -----------------------------------------------------
            // REGISTER
            // -----------------------------------------------------
            crewMembers.Add(crew);

            Debug.LogFormat("RegisterCrew: Registered runtime crew | GO: {0} | ID: {1} | TotalRegistered: {2}", crew.name, crew.crewData.crewId, crewMembers.Count);
            return true;
        }

        // =========================================================
        // NO CREWDATA => create a new persistent identity
        // =========================================================
        string newCrewId = GenerateCrewId();
        string newCrewName = GenerateCrewName(crewData.Count + 1);

        CrewData newData = new CrewData(newCrewId, newCrewName);

        crew.crewData = newData;
        crewData.Add(newData);
        crewMembers.Add(crew);

        Debug.LogFormat("RegisterCrew: NEW CREW DATA CREATED BY SCENE INSTANCE | GO: {0} | Name: {1} | ID: {2}", crew.name, newCrewName, newCrewId);

        return true;
    }

    // Debug helper to dump registry state (callable from code)
    public void DumpCrewRegistry()
    {
        Debug.Log("===== DUMP CREW REGISTRY =====");
        if (crewMembers == null)
        {
            Debug.Log("crewMembers == null");
        }
        else
        {
            Debug.Log("Registered runtime crew: " + crewMembers.Count);
            for (int i = 0; i < crewMembers.Count; i++)
            {
                var cm = crewMembers[i];
                if (cm == null)
                {
                    Debug.LogFormat("{0}: NULL entry", i);
                    continue;
                }

                string id = cm.crewData != null ? cm.crewData.crewId : "<null>";
                string name = cm.crewData != null ? cm.crewData.crewName : "<null>";
                bool vp = cm.IsVoyagePersistent();
                Debug.LogFormat("{0}: GO='{1}' instanceId={2} | crewId='{3}' | crewName='{4}' | voyagePersistent={5} | currentJob={6}",
                    i,
                    cm.name,
                    cm.gameObject.GetInstanceID(),
                    id,
                    name,
                    vp,
                    cm.currentJob
                );
            }
        }

        if (crewData == null)
        {
            Debug.Log("crewData == null");
        }
        else
        {
            Debug.Log("Persistent CrewData entries: " + crewData.Count);
            for (int i = 0; i < crewData.Count; i++)
            {
                var d = crewData[i];
                if (d == null)
                {
                    Debug.LogFormat("{0}: NULL data", i);
                    continue;
                }

                Debug.LogFormat("{0}: crewId={1} | crewName={2} | onVoyage={3} | islandJob={4}",
                    i,
                    d.crewId,
                    d.crewName,
                    d.isOnVoyage,
                    d.islandJob
                );
            }
        }
        Debug.Log("===== END DUMP =====");
    }


    // =========================================================
    // UNREGISTER PHYSICAL CREW
    // =========================================================

    public void UnregisterCrew(
        CrewMovement crew
    )
    {
        if (crew == null)
        {
            return;
        }

        crewMembers.Remove(
            crew
        );
    }


    // =========================================================
    // FIND CREW BY ID
    // =========================================================

    public CrewData GetCrewDataById(
        string crewId
    )
    {
        if (
            string.IsNullOrEmpty(
                crewId
            )
        )
        {
            return null;
        }


        foreach (
            CrewData data
            in crewData
        )
        {
            if (
                data != null &&
                data.crewId == crewId
            )
            {
                return data;
            }
        }


        return null;
    }


    // =========================================================
    // FIND PHYSICAL CREW BY ID
    // =========================================================

    public CrewMovement GetCrewById(
        string crewId
    )
    {
        if (
            string.IsNullOrEmpty(
                crewId
            )
        )
        {
            return null;
        }


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (
                crew == null ||
                crew.crewData == null
            )
            {
                continue;
            }


            if (
                crew.crewData.crewId ==
                crewId
            )
            {
                return crew;
            }
        }


        return null;
    }


    // =========================================================
    // SET ISLAND JOB
    // =========================================================

    public void SetIslandJob(
    string crewId,
    CrewIslandJob job
)
    {
        CrewData data =
            GetCrewDataById(
                crewId
            );

        if (data == null)
        {
            Debug.LogWarning(
                "Could not find crew ID: " +
                crewId
            );

            return;
        }


        // =========================================================
        // CREWDATA IS THE SOURCE OF TRUTH
        // =========================================================

        data.islandJob =
            job;


        // =========================================================
        // CLEAR STATE THAT NO LONGER APPLIES
        // =========================================================

        if (job != CrewIslandJob.Fishing)
        {
            data.fishingSpotIndex =
                -1;
        }

        if (job != CrewIslandJob.Building)
        {
            data.assignedBuildingName =
                "";

            data.wasWorking =
                false;
        }


        Debug.Log(
            "CREW ISLAND JOB | " +
            data.crewName +
            " | " +
            data.crewId +
            " | " +
            job
        );
    }


    // =========================================================
    // ASSIGN VOYAGE
    // =========================================================

    public void SetVoyageState(
        string crewId,
        string shipId,
        VoyageRole role
    )
    {
        CrewData data =
            GetCrewDataById(
                crewId
            );


        if (data == null)
        {
            Debug.LogWarning(
                "Cannot assign voyage. " +
                "Crew ID not found: " +
                crewId
            );

            return;
        }


        data.isOnVoyage =
            true;

        data.assignedShipId =
            shipId;

        data.shipRole =
            role;


        // Remember what the crew member was doing
        // before going on the voyage.
        data.previousIslandJob =
            data.islandJob;

        // Crew is no longer working on the island
        // while away on the voyage.
        data.islandJob =
            CrewIslandJob.Idle;


        Debug.Log(
            "CREW ASSIGNED TO VOYAGE | " +
            data.crewName +
            " | ID: " +
            data.crewId +
            " | Ship: " +
            shipId +
            " | Role: " +
            role
        );
    }


    // =========================================================
    // CLEAR VOYAGE
    // =========================================================

    public void ClearVoyageState(
    string crewId
)
    {
        CrewData data =
            GetCrewDataById(
                crewId
            );


        if (data == null)
        {
            Debug.LogWarning(
                "Cannot clear voyage. " +
                "Crew ID not found: " +
                crewId
            );

            return;
        }


        data.isOnVoyage =
            false;

        data.assignedShipId =
            "";

        data.shipRole =
            VoyageRole.Sailor;


        // Restore the island job the crew member
        // had before going on the voyage.
        data.islandJob =
            data.previousIslandJob;


        Debug.Log(
            "CREW VOYAGE CLEARED | " +
            data.crewName +
            " | ID: " +
            data.crewId +
            " | Restored Job: " +
            data.islandJob
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupCrewReferences()
    {
        if (crewMembers == null)
        {
            crewMembers =
                new List<CrewMovement>();

            return;
        }


        for (
            int i =
                crewMembers.Count - 1;

            i >= 0;

            i--
        )
        {
            if (
                crewMembers[i] == null
            )
            {
                crewMembers.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // DEBUG IDENTITY
    // =========================================================

    public void PrintCrewIdentity()
    {
        Debug.Log(
            "========== CREW IDENTITY =========="
        );


        CleanupCrewReferences();


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (
                crew == null ||
                crew.crewData == null
            )
            {
                continue;
            }


            CrewData data =
                crew.crewData;


            Debug.Log(
                data.crewName +
                " | ID: " +
                data.crewId +
                " | Island Job: " +
                data.islandJob +
                " | On Voyage: " +
                data.isOnVoyage +
                " | Ship: " +
                data.assignedShipId +
                " | Role: " +
                data.shipRole
            );
        }


        Debug.Log(
            "=================================="
        );
    }


    // =========================================================
    // MISSION CREW
    // =========================================================

    public void SendCrewToMission(int amount)
    {
        CleanupCrewReferences();

        if (MissionManager.Instance == null)
        {
            Debug.LogError(
                "Cannot send crew to mission: MissionManager missing."
            );

            return;
        }

        if (MissionManager.Instance.currentMission == null)
        {
            Debug.LogError(
                "Cannot send crew to mission: No mission selected."
            );

            return;
        }

        // =========================================================
        // GET THE ACTIVE VOYAGE
        // =========================================================

        VoyageData voyage =
            MissionManager.Instance
                .GetActiveVoyage();

        if (voyage == null)
        {
            Debug.LogError(
                "Cannot send crew to mission: " +
                "No active voyage found."
            );

            return;
        }

        // =========================================================
        // RESET DOCK ARRIVAL
        // =========================================================

        crewArrived = 0;

        crewRequired =
            voyage.crew.Count;

        int assigned = 0;

        // =========================================================
        // USE THE VOYAGE MANIFEST
        //
        // IMPORTANT:
        // We DO NOT pick random/available crew here.
        //
        // VoyageData already contains the exact crew IDs.
        // =========================================================

        foreach (
            VoyageCrewData voyageCrew
            in voyage.crew
        )
        {
            if (voyageCrew == null)
            {
                continue;
            }

            CrewMovement crew =
                GetCrewById(
                    voyageCrew.crewId
                );

            if (crew == null)
            {
                Debug.LogError(
                    "VOYAGE CREW NOT FOUND | " +
                    "ID: " +
                    voyageCrew.crewId +
                    " | Name: " +
                    voyageCrew.crewName
                );

                continue;
            }

            // =====================================================
            // MARK CREW AS MISSION CREW
            // =====================================================

            crew.assignedToMission =
                true;

            crew.currentJob =
                CrewJob.Mission;

            // =====================================================
            // PERSISTENT CREW IDENTITY
            // =====================================================

            if (crew.crewData != null)
            {
                crew.crewData.isOnVoyage =
                    true;

                crew.crewData.assignedShipId =
                    voyage.shipId;

                crew.crewData.shipRole =
                    voyageCrew.shipRole;

                crew.crewData.islandJob =
                    CrewIslandJob.Idle;
            }

            // =====================================================
            // SEND THIS EXACT PIRATE TO THE DOCK
            // =====================================================

            if (dockPoint != null)
            {
                crew.GoToDock(
                    dockPoint
                );
            }
            else
            {
                Debug.LogWarning(
                    "CrewManager: Dock point is missing."
                );
            }

            assigned++;

            Debug.Log(
                "CREW SENT TO DOCK | " +
                voyageCrew.crewName +
                " | ID: " +
                voyageCrew.crewId +
                " | Role: " +
                voyageCrew.shipRole
            );
        }

        // =========================================================
        // UPDATE REQUIRED CREW
        // =========================================================

        crewRequired =
            assigned;

        // =========================================================
        // DEBUG
        // =========================================================

        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "VOYAGE CREW SENT"
        );

        Debug.Log(
            "Ship: " +
            voyage.shipId
        );

        Debug.Log(
            "Crew Required: " +
            crewRequired
        );

        Debug.Log(
            "Crew Assigned: " +
            assigned
        );

        Debug.Log(
            "========================================"
        );

        PrintCrewStates();
    }


    // =========================================================
    // CREW ARRIVAL AT SHIP
    // =========================================================

    public void CrewReachedDock()
    {
        crewArrived++;
        

        Debug.Log(
            "Crew Arrived: " +
            crewArrived +
            "/" +
            crewRequired
        );


        if (
            crewArrived >=
            crewRequired
        )
        {
            crewArrived = 0;


            PrintCrewStates();


            if (
                MissionManager.Instance != null
            )
            {
                MissionManager.Instance
                    .BeginShipDeparture();
            }
        }
    }


    // =========================================================
    // RETURN CREW
    // =========================================================

    public void ReturnCrew()
    {
        CleanupCrewReferences();


        Debug.Log(
            "===== RETURNING VOYAGE CREW ====="
        );


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }


            if (
                crew.crewData == null ||
                !crew.crewData.isOnVoyage
            )
            {
                continue;
            }


            string crewId =
                crew.crewData.crewId;


            ClearVoyageState(
                crewId
            );


            crew.ReconnectToIsland(
                this
            );


            crew.assignedToMission =
                false;


            crew.currentJob =
                CrewJob.Idle;


            if (
                dockPoint != null
            )
            {
                crew.transform.position =
                    dockPoint.position;
            }


            crew.gameObject.SetActive(
                true
            );


            crew.ReturnToIsland();


            Debug.Log(
                "CREW RETURNED | " +
                crew.crewData.crewName +
                " | ID: " +
                crewId
            );
        }


        PrintCrewStates();
    }


    // =========================================================
    // FISHING
    // =========================================================

    public void AddFishingCrew()
    {
        CleanupCrewReferences();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ADD FISHER: GameManager missing.");
            return;
        }

        if (fishingSpots == null || fishingSpots.Length == 0)
        {
            Debug.Log("ADD FISHER: No fishing spots available.");
            return;
        }

        if (GameManager.Instance.availableCrew <= 0)
        {
            Debug.Log("ADD FISHER: No available crew.");
            return;
        }

        // Find an actually free fishing spot.
        Transform freeSpot = null;
        int freeSpotIndex = -1;

        for (int i = 0; i < fishingSpots.Length; i++)
        {
            if (fishingSpots[i] == null)
                continue;

            bool occupied = false;

            foreach (CrewMovement otherCrew in crewMembers)
            {
                if (otherCrew == null)
                    continue;

                if (otherCrew.currentJob == CrewJob.Fishing &&
                    otherCrew.assignedFishingSpot == fishingSpots[i])
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                freeSpot = fishingSpots[i];
                freeSpotIndex = i;
                break;
            }
        }

        if (freeSpot == null)
        {
            Debug.Log("ADD FISHER: No free fishing spots.");
            return;
        }

        // Find genuinely idle island crew.
        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null || crew.crewData == null)
                continue;

            if (crew.crewData.isOnVoyage)
                continue;

            if (crew.currentJob != CrewJob.Idle)
                continue;

            // Assign the job.
            crew.currentJob = CrewJob.Fishing;
            crew.assignedFishingSpot = freeSpot;

            crew.crewData.fishingSpotIndex = freeSpotIndex;
            crew.crewData.wasWorking = false;

            SetIslandJob(
                crew.crewData.crewId,
                CrewIslandJob.Fishing
            );

            fishingAssigned++;

            Debug.Log(
                "FISHER ASSIGNED | " +
                crew.crewData.crewName +
                " | ID: " +
                crew.crewData.crewId +
                " | Spot: " +
                freeSpot.name +
                " | Assigned: " +
                fishingAssigned +
                " | Working: " +
                fishingCrew +
                " | Available: " +
                GameManager.Instance.availableCrew
            );

            // Tell CrewMovement to actually start moving.
            crew.AssignFishingJob(freeSpot);

            RecalculateAvailableCrew();

            return;
        }

        Debug.Log("ADD FISHER: No idle crew found.");
    }


    public void FishermanStartedWork()
    {
        fishingCrew++;

        Debug.Log(
            "FISHERMAN STARTED WORK | Working Fishermen: " +
            fishingCrew
        );
    }


    public void RemoveFishingCrew()
    {
        Debug.Log("=== REMOVE FISHERMAN BUTTON PRESSED ===");

        CleanupCrewReferences();

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("REMOVE FISHER: GameManager missing.");
            return;
        }

        // Find a fisherman to remove.
        // Prefer one who is actually working.
        CrewMovement selectedCrew = null;

        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null || crew.crewData == null)
                continue;

            if (crew.crewData.isOnVoyage)
                continue;

            if (crew.currentJob != CrewJob.Fishing)
                continue;

            if (crew.crewData.wasWorking)
            {
                selectedCrew = crew;
                break;
            }

            // Keep as fallback in case they're still walking.
            if (selectedCrew == null)
            {
                selectedCrew = crew;
            }
        }

        if (selectedCrew == null)
        {
            Debug.Log("REMOVE FISHER: No fisherman found.");
            return;
        }

        string crewName = selectedCrew.crewData.crewName;
        string crewId = selectedCrew.crewData.crewId;

        Debug.Log(
            "REMOVING FISHER | " +
            crewName +
            " | ID: " +
            crewId
        );

        // If they had actually started working,
        // remove them from the working fisherman count.
        if (selectedCrew.crewData.wasWorking)
        {
            fishingCrew = Mathf.Max(
                0,
                fishingCrew - 1
            );
        }

        // Remove the assignment count.
        fishingAssigned = Mathf.Max(
            0,
            fishingAssigned - 1
        );

        // Clear the fishing spot.
        selectedCrew.assignedFishingSpot = null;

        // Clear persistent fishing data.
        selectedCrew.crewData.fishingSpotIndex = -1;
        selectedCrew.crewData.wasWorking = false;

        // Change persistent island job.
        SetIslandJob(
            crewId,
            CrewIslandJob.Idle
        );

        // Return crew to idle.
        selectedCrew.currentJob = CrewJob.Idle;

        selectedCrew.ReturnToIsland();

        // Recalculate instead of manually guessing available crew.
        RecalculateAvailableCrew();

        Debug.Log(
            "FISHER REMOVED | " +
            crewName +
            " | Working: " +
            fishingCrew +
            " | Assigned: " +
            fishingAssigned +
            " | Available: " +
            GameManager.Instance.availableCrew
        );
    }


    // =========================================================
    // BUILDERS
    // =========================================================

    public void AddBuilderCrew()
    {
        if (
            GameManager.Instance
                .availableCrew <= 0
        )
        {
            return;
        }


        if (
            activeConstruction == null
        )
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    "NO BUILDINGS TO CONSTRUCT"
                );

            return;
        }


        CleanupCrewReferences();


        foreach (CrewMovement crew in crewMembers)
        {
            if (
                crew == null ||
                crew.crewData == null
            )
            {
                continue;
            }

            // Do NOT assign voyage crew to island jobs.
            if (crew.crewData.isOnVoyage)
            {
                continue;
            }

            // Only use genuinely idle island crew.
            if (crew.currentJob != CrewJob.Idle)
            {
                continue;
            }

            builderAssigned++;

            crew.AssignBuilderJob(
                activeConstruction.builderSpot
            );

            crew.crewData.assignedBuildingName =
                activeConstruction.buildingName;

            crew.crewData.wasWorking =
                false;

            SetIslandJob(
                crew.crewData.crewId,
                CrewIslandJob.Building
            );

            Debug.Log(
                "BUILDER ASSIGNED | " +
                crew.crewData.crewName +
                " | ID: " +
                crew.crewData.crewId
            );

            break;
        }
    }


    public void BuilderStartedWork()
    {
        builderCrew++;

        Debug.Log(
            "Builder Arrived: " +
            builderCrew
        );

        // Save that this builder has actually reached
        // the building and started working.
        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null || crew.crewData == null)
            {
                continue;
            }

            if (
                crew.currentJob == CrewJob.Building &&
                crew.IsWorking()
            )
            {
                crew.crewData.wasWorking = true;

                Debug.Log(
                    "BUILDER WORK STATE SAVED | " +
                    crew.crewData.crewName +
                    " | Building: " +
                    crew.crewData.assignedBuildingName
                );

                break;
            }
        }
    }


    public void RemoveBuilderCrew()
    {
        if (
            builderAssigned <= 0
        )
        {
            return;
        }


        CleanupCrewReferences();


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }


            if (
                crew.currentJob ==
                CrewJob.Building
            )
            {
                if (
                    crew.IsWorking()
                )
                {
                    builderCrew--;
                }


                builderAssigned--;


                GameManager.Instance
                    .availableCrew++;
                

                crew.ReturnToIsland();


                SetIslandJob(
                    crew.crewData.crewId,
                    CrewIslandJob.Idle
                );


                break;
            }
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    public void PrintCrewStates()
    {
        Debug.Log(
            "----- CREW STATES -----"
        );


        CleanupCrewReferences();


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }


            Debug.Log(
                crew.name +
                " | ID: " +
                (
                    crew.crewData != null
                        ? crew.crewData.crewId
                        : "NONE"
                ) +
                " | Name: " +
                (
                    crew.crewData != null
                        ? crew.crewData.crewName
                        : "NONE"
                ) +
                " | Job: " +
                crew.currentJob +
                " | Island Job: " +
                (
                    crew.crewData != null
                        ? crew.crewData.islandJob.ToString()
                        : "NONE"
                ) +
                " | AssignedToMission: " +
                crew.assignedToMission +
                " | OnVoyage: " +
                (
                    crew.crewData != null
                        ? crew.crewData.isOnVoyage
                        : false
                ) +
                " | ShipRole: " +
                (
                    crew.crewData != null
                        ? crew.crewData.shipRole.ToString()
                        : "NONE"
                )
            );
        }
    }


    // =========================================================
    // DEBUG TEXT
    // =========================================================

    public string GetCrewDebugText()
    {
        CleanupCrewReferences();


        string text = "";
        

        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }


            text +=
                crew.name +
                ": " +
                crew.GetDebugState() +
                "\n";
        }


        return text;
    }


    // =========================================================
    // SHIP STATE
    // =========================================================

    public bool IsShipAway()
    {
        if (
            ShipManager.Instance == null
        )
        {
            return false;
        }


        ShipState ship =
            ShipManager.Instance
                .GetShip("ship_001");


        if (ship == null)
        {
            return false;
        }


        return ship.onVoyage;
    }


    // =========================================================
    // OLD HIDING SYSTEM
    // =========================================================

    public void HideIslandCrewIfShipAway()
    {
        if (
            ShipManager.Instance == null
        )
        {
            return;
        }


        ShipState ship =
            ShipManager.Instance
                .GetShip("ship_001");


        if (
            ship == null ||
            !ship.onVoyage
        )
        {
            return;
        }


        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (crew == null)
            {
                continue;
            }


            if (
                crew.crewData != null &&
                crew.crewData.isOnVoyage
            )
            {
                crew.gameObject.SetActive(
                    false
                );
            }
        }
    }

    public void AssignBuildersToBuilding(
    BuildingClickable building
)
    {
        if (building == null)
        {
            Debug.LogWarning(
                "Cannot assign builders: building is null."
            );

            return;
        }

        if (building.builderSpot == null)
        {
            Debug.LogWarning(
                "Cannot assign builders: " +
                building.buildingName +
                " has no builder spot."
            );

            return;
        }

        activeConstruction = building;

        builderCrew = 0;
        builderAssigned = 0;

        Debug.Log(
            "CONSTRUCTION READY | " +
            building.buildingName +
            " | Builders assigned: 0" +
            " | Available crew: " +
            GameManager.Instance.availableCrew
        );
    }

    private void HideIslandCrewAfterLoad()
    {
        if (crewMembers == null)
        {
            return;
        }

        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null)
            {
                continue;
            }

            if (
                crew.crewData != null &&
                crew.crewData.isOnVoyage
            )
            {
                crew.HideIslandRepresentation();

                Debug.Log(
                    "ISLAND CREW HIDDEN | " +
                    crew.crewData.crewName +
                    " | ID: " +
                    crew.crewData.crewId
                );
            }
        }
    }

    public void RestoreIslandCrewVisibility()
    {
        if (crewMembers == null)
        {
            return;
        }

        Debug.Log(
            "===== RESTORING ISLAND CREW ====="
        );

        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null)
            {
                continue;
            }

            if (crew.crewData == null)
            {
                continue;
            }

            if (crew.crewData.isOnVoyage)
            {
                crew.HideIslandRepresentation();

                Debug.Log(
                    "HIDING VOYAGE CREW ON ISLAND | " +
                    crew.crewData.crewName +
                    " | ID: " +
                    crew.crewData.crewId
                );
            }
            else
            {
                crew.ShowIslandRepresentation();
            }
        }
    }

    public void FinishConstructionCrew(string buildingName)
    {
        CleanupCrewReferences();

        int returnedBuilders = 0;

        foreach (CrewMovement crew in crewMembers)
        {
            if (crew == null || crew.crewData == null)
            {
                continue;
            }

            CrewData data = crew.crewData;

            /*
             * CrewData is the authoritative state.
             *
             * Do NOT use crew.currentJob here.
             */
            if (data.islandJob != CrewIslandJob.Building)
            {
                continue;
            }

            /*
             * Only return builders assigned to this building.
             *
             * This prevents a builder assigned to another
             * construction project from being released.
             */
            if (
                !string.IsNullOrEmpty(data.assignedBuildingName) &&
                data.assignedBuildingName != buildingName
            )
            {
                continue;
            }

            /*
             * Clear the persistent state FIRST.
             */
            data.islandJob =
                CrewIslandJob.Idle;

            data.assignedBuildingName =
                "";

            data.wasWorking =
                false;

            /*
             * Now update the runtime representation.
             */
            crew.ReturnToIsland();

            returnedBuilders++;

            Debug.Log(
                "BUILDER RETURNED | " +
                data.crewName +
                " | ID: " +
                data.crewId +
                " | " +
                buildingName
            );
        }

        /*
         * Do NOT manually add to availableCrew.
         *
         * Recalculate from CrewData instead.
         */
        builderCrew = 0;
        builderAssigned = 0;

        activeConstruction = null;

        RecalculateAvailableCrew();

        Debug.Log(
            "CONSTRUCTION CREW FINISHED | " +
            buildingName +
            " | Returned: " +
            returnedBuilders +
            " | Available Crew: " +
            GameManager.Instance.availableCrew
        );
    }

    // =========================================================
    // RESTORE ISLAND STATE AFTER SCENE LOAD
    // =========================================================

    public void RestoreIslandState()
    {
        CleanupCrewReferences();

        Debug.Log(
            "===== RESTORING ISLAND STATE ====="
        );


        // =========================================================
        // RESET RUNTIME COUNTERS
        // =========================================================

        fishingCrew = 0;
        builderCrew = 0;

        fishingAssigned = 0;
        builderAssigned = 0;

        activeConstruction = null;


        // =========================================================
        // FIND ACTIVE CONSTRUCTION
        // =========================================================

        BuildingClickable[] islandBuildings =
            FindObjectsOfType<BuildingClickable>();


        foreach (
            BuildingClickable building
            in islandBuildings
        )
        {
            if (building == null)
            {
                continue;
            }


            BuildingPersistentState state =
                GameManager.Instance.GetBuildingState(
                    building.buildingName
                );


            if (
                state != null &&
                state.upgrading
            )
            {
                activeConstruction =
                    building;


                Debug.Log(
                    "ACTIVE CONSTRUCTION RESTORED | " +
                    building.buildingName
                );


                foreach (CrewMovement crew in crewMembers)
                {
                    if (crew == null || crew.crewData == null)
                    {
                        continue;
                    }

                    if (
    crew.crewData.islandJob == CrewIslandJob.Building &&
    crew.crewData.assignedBuildingName == building.buildingName
)
                    {
                        builderAssigned++;

                        if (crew.crewData.wasWorking)
                        {
                            crew.SetWorkingState(true);
                            builderCrew++;
                        }

                        Debug.Log(
                            "BUILDER STATUS RESTORED | " +
                            crew.crewData.crewName +
                            " | ID: " +
                            crew.crewData.crewId +
                            " | Building: " +
                            crew.crewData.assignedBuildingName
                        );
                    }
                }

                break;
            }
        }


        // =========================================================
        // RESTORE CREW
        // =========================================================

        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (
                crew == null ||
                crew.crewData == null
            )
            {
                continue;
            }


            CrewData data =
                crew.crewData;


            // =====================================================
            // VOYAGE CREW
            // =====================================================

            if (data.isOnVoyage)
            {
                crew.currentJob =
                    CrewJob.Voyage;

                crew.assignedToMission =
                    true;

                crew.HideIslandRepresentation();

                Debug.Log(
                    "VOYAGE CREW REMAINS HIDDEN | " +
                    data.crewName +
                    " | ID: " +
                    data.crewId
                );

                continue;
            }


            // =====================================================
            // ISLAND CREW
            // =====================================================

            crew.assignedToMission =
                false;

            crew.ShowIslandRepresentation();


            // =====================================================
            // FISHING
            // =====================================================

            if (
                data.islandJob ==
                CrewIslandJob.Fishing
            )
            {
                Transform fishingSpot = null;

                // First try to restore the exact fishing spot
                // this crew was using before the scene changed.
                if (
                    data.fishingSpotIndex >= 0 &&
                    data.fishingSpotIndex < fishingSpots.Length
                )
                {
                    fishingSpot =
                        fishingSpots[data.fishingSpotIndex];
                }

                // If the saved spot cannot be found,
                // find another available fishing spot.
                if (fishingSpot == null)
                {
                    fishingSpot =
                        FindAvailableFishingSpot(crew);
                }

                if (fishingSpot != null)
                {
                    fishingAssigned++;

                    // Restore the crew's fishing assignment.
                    crew.currentJob =
                        CrewJob.Fishing;

                    crew.AssignFishingJob(
                        fishingSpot
                    );

                    // IMPORTANT:
                    // Restore whether the crew had actually reached
                    // the fishing spot before leaving the island.
                    if (data.wasWorking)
                    {
                        crew.transform.position =
                            fishingSpot.position;

                        crew.SetWorkingState(true);

                        // Make absolutely sure the runtime fishing
                        // counter is restored.
                        fishingCrew++;

                        Debug.Log(
                            "FISHER RESTORED WORKING | " +
                            data.crewName +
                            " | ID: " +
                            data.crewId +
                            " | Spot: " +
                            fishingSpot.name
                        );
                    }
                    else
                    {
                        // Crew was assigned to fishing but had not
                        // reached the spot yet.
                        crew.SetWorkingState(false);

                        Debug.Log(
                            "FISHER RESTORED WALKING | " +
                            data.crewName +
                            " | ID: " +
                            data.crewId +
                            " | Spot: " +
                            fishingSpot.name
                        );
                    }
                }
                else
                {
                    // Keep the saved job even if a spot cannot currently
                    // be found. The crew can be assigned later.
                    crew.currentJob =
                        CrewJob.Fishing;

                    crew.SetWorkingState(false);

                    Debug.LogWarning(
                        "FISHING SPOT UNAVAILABLE | " +
                        data.crewName +
                        " | ID: " +
                        data.crewId
                    );
                }

                continue;
            }




            // =====================================================
            // BUILDING
            // =====================================================

            if (
                data.islandJob ==
                CrewIslandJob.Building
            )
                {
                    BuildingClickable assignedBuilding = null;

                    // Find the exact building this crew member was assigned to.
                    if (!string.IsNullOrEmpty(data.assignedBuildingName))
                    {
                        foreach (
                            BuildingClickable building
                            in islandBuildings
                        )
                        {
                            if (
                                building != null &&
                                building.buildingName ==
                                data.assignedBuildingName
                            )
                            {
                                assignedBuilding = building;
                                break;
                            }
                        }
                    }

                    // Fall back to the active construction if needed.
                    if (
        assignedBuilding != null &&
        assignedBuilding.builderSpot != null
    )
                    {
                        // =====================================================
                        // PERSISTENT STATE — SOURCE OF TRUTH
                        // =====================================================

                        data.islandJob =
                            CrewIslandJob.Building;

                        data.assignedBuildingName =
                            assignedBuilding.buildingName;


                        // =====================================================
                        // RUNTIME STATE — SYNCHRONIZE TO CREWDATA
                        // =====================================================

                        activeConstruction =
                            assignedBuilding;

                        builderAssigned++;

                        if (data.wasWorking)
                        {
                            // They were already working before leaving the island.
                            // Restore them directly at their work position.

                            crew.transform.position =
                                assignedBuilding.builderSpot.position;

                            crew.AssignBuilderJob(
                                assignedBuilding.builderSpot
                            );

                            crew.SetWorkingState(true);

                            builderCrew++;

                            Debug.Log(
                                "BUILDER RESTORED AT WORK POSITION | " +
                                data.crewName +
                                " | ID: " +
                                data.crewId +
                                " | Building: " +
                                assignedBuilding.buildingName
                            );
                        }
                        else
                        {
                            // They were assigned to building but had not reached
                            // the work position yet. Let them walk there normally.

                            crew.AssignBuilderJob(
                                assignedBuilding.builderSpot
                            );

                            Debug.Log(
                                "BUILDER RESTORED - WALKING TO WORK POSITION | " +
                                data.crewName +
                                " | ID: " +
                                data.crewId
                            );
                        }


                        Debug.Log(
                            "BUILDER RESTORED | " +
                            data.crewName +
                            " | ID: " +
                            data.crewId +
                            " | Building: " +
                            assignedBuilding.buildingName +
                            " | IslandJob: " +
                            data.islandJob +
                            " | WasWorking: " +
                            data.wasWorking
                        );
                    }
                    else
                    {
                        // The building no longer exists or has no builder spot.
                        data.islandJob =
                            CrewIslandJob.Idle;

                        data.assignedBuildingName =
                            "";

                        data.wasWorking =
                            false;

                        crew.currentJob =
                            CrewJob.Idle;

                        crew.assignedBuilderSpot =
                            null;

                        Debug.LogWarning(
                            "BUILDER COULD NOT BE RESTORED | " +
                            data.crewName +
                            " | Building: " +
                            data.assignedBuildingName
                        );
                    }

                    continue;
                }


                // =====================================================
                // IDLE
                // =====================================================

                data.islandJob =
                    CrewIslandJob.Idle;


                crew.currentJob =
                    CrewJob.Idle;


                crew.assignedToMission =
                    false;


                crew.ShowIslandRepresentation();


                if (
                    crew.waypoints != null &&
                    crew.waypoints.Length > 0
                )
                {
                    crew.PickNewTargetPublic();
                }

            }




            // =========================================================
            // RESTORE AVAILABLE CREW
            // =========================================================

            RecalculateAvailableCrew();


            Debug.Log(
                "ISLAND STATE RESTORED | " +
                "Builders: " +
                builderCrew +
                " | Fishing: " +
                fishingCrew +
                " | Available: " +
                GameManager.Instance.availableCrew +
                " | Active Construction: " +
                (
                    activeConstruction != null
                        ? activeConstruction.buildingName
                        : "NONE"
                )
            );

        }

    // =========================================================
    // FIND AVAILABLE FISHING SPOT
    // =========================================================

    private Transform FindAvailableFishingSpot(
        CrewMovement currentCrew
    )
    {
        if (
            fishingSpots == null ||
            fishingSpots.Length == 0
        )
        {
            return null;
        }


        foreach (
            Transform spot
            in fishingSpots
        )
        {
            if (spot == null)
            {
                continue;
            }


            bool occupied = false;


            foreach (
                CrewMovement crew
                in crewMembers
            )
            {
                if (
                    crew == null ||
                    crew == currentCrew
                )
                {
                    continue;
                }


                if (
                    crew.assignedFishingSpot == 
                    spot
                )
                {
                    occupied = true;
                    break;
                }
            }


            if (!occupied)
            {
                return spot;
            }
        }


        return null;
    }

    private void OnSceneLoaded(
    UnityEngine.SceneManagement.Scene scene,
    UnityEngine.SceneManagement.LoadSceneMode mode
)
    {
        if (scene.name == "IslandScene")
        {
            StartCoroutine(
                RestoreIslandStateDelayed()
            );
        }
    }

    private System.Collections.IEnumerator RestoreIslandStateDelayed()
    {
        yield return null;

        yield return null;

        RestoreIslandState();
    }

    public void RecalculateAvailableCrew()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        int islandCrew = 0;
        int workingCrew = 0;

        foreach (CrewData data in crewData)
        {
            if (data == null)
            {
                continue;
            }

            // Ignore empty/uninitialised crew records.
            if (string.IsNullOrEmpty(data.crewId))
            {
                continue;
            }

            Debug.Log(
                "CHECKING CREW | " +
                data.crewName +
                " | ID: " +
                data.crewId +
                " | OnVoyage: " +
                data.isOnVoyage +
                " | IslandJob: " +
                data.islandJob
            );


            // =====================================================
            // VOYAGE CREW
            // =====================================================

            if (data.isOnVoyage)
            {
                Debug.Log(
                    "SKIPPED - ON VOYAGE | " +
                    data.crewName
                );

                continue;
            }


            islandCrew++;
            

            // =====================================================
            // WORKING CREW
            // =====================================================

            if (
                data.islandJob == CrewIslandJob.Fishing ||
                data.islandJob == CrewIslandJob.Building
            )
            {
                workingCrew++;

                Debug.Log(
                    "SKIPPED - WORKING | " +
                    data.crewName
                );

                continue;
            }


            // =====================================================
            // AVAILABLE CREW
            // =====================================================

            Debug.Log(
                "COUNTED AS AVAILABLE | " +
                data.crewName
            );
        }


        GameManager.Instance.availableCrew =
            Mathf.Clamp(
                islandCrew - workingCrew,
                0,
                GameManager.Instance.maxCrew
            );


        Debug.Log(
            "AVAILABLE CREW RESULT | " +
            "Island Crew: " +
            islandCrew +
            " | Working Crew: " +
            workingCrew +
            " | Available: " +
            GameManager.Instance.availableCrew
        );
    }

    public void LoadCrewData(
    List<CrewData> savedCrewData
)
    {
        if (savedCrewData == null)
        {
            Debug.LogWarning(
                "NO SAVED CREW DATA FOUND"
            );

            return;
        }


        // =========================================================
        // REPLACE CURRENT CREW DATA
        // =========================================================

        crewData =
            savedCrewData;


        Debug.Log(
            "CREW DATA LOADED | Count: " +
            crewData.Count
        );


        // =========================================================
        // RECONNECT SCENE CREW
        // =========================================================

        foreach (
            CrewMovement crew
            in crewMembers
        )
        {
            if (
                crew == null
            )
            {
                continue;
            }


            if (
                crew.crewData == null
            )
            {
                continue;
            }


            CrewData savedData =
                GetCrewDataById(
                    crew.crewData.crewId
                );


            if (
                savedData == null
            )
            {
                continue;
            }


            crew.crewData =
                savedData;


            // =====================================================
            // CREW ON VOYAGE
            // =====================================================

            if (
                savedData.isOnVoyage
            )
            {
                crew.assignedToMission =
                    true;

                crew.currentJob =
                    CrewJob.Mission;

                crew.gameObject.SetActive(
                    false
                );


                Debug.Log(
                    "CREW RESTORED ON VOYAGE | " +
                    savedData.crewName +
                    " | Ship: " +
                    savedData.assignedShipId +
                    " | Role: " +
                    savedData.shipRole
                );

                continue;
            }


            // =====================================================
            // CREW ON ISLAND
            // =====================================================

            crew.assignedToMission =
                false;


            switch (
                savedData.islandJob
            )
            {
                case CrewIslandJob.Fishing:

                    crew.currentJob =
                        CrewJob.Fishing;

                    break;


                case CrewIslandJob.Building:

                    crew.currentJob =
                        CrewJob.Building;

                    break;


                default:

                    crew.currentJob =
                        CrewJob.Idle;

                    break;
            }


            Debug.Log(
                "CREW DATA RECONNECTED AFTER LOAD | " +
                savedData.crewName +
                " | ID: " +
                savedData.crewId +
                " | On Voyage: " +
                savedData.isOnVoyage +
                " | Island Job: " +
                savedData.islandJob
            );
        }
    }
}