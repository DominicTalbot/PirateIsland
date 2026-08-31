using System.Collections.Generic;
using UnityEngine;

public class VoyageCrewManager : MonoBehaviour
{
    [Header("Crew")]
    public GameObject crewPrefab;
    public Transform crewSpawn;

    [Header("Crew Stations")]
    public Transform captainPosition;
    public Transform lookoutPosition;

    public Transform sailPosition1;
    public Transform sailPosition2;
    public Transform sailPosition3;

    private readonly List<GameObject> spawnedCrew =
        new List<GameObject>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        StartCoroutine(
            SpawnCrewWhenReady()
        );
    }


    // =========================================================
    // WAIT FOR VOYAGE
    // =========================================================

    private System.Collections.IEnumerator SpawnCrewWhenReady()
    {
        yield return null;
        yield return null;

        SpawnVoyageCrew();
    }


    // =========================================================
    // SPAWN VOYAGE CREW
    // =========================================================

    private void SpawnVoyageCrew()
    {
        if (crewPrefab == null)
        {
            Debug.LogError(
                "VoyageCrewManager: Crew Prefab is missing."
            );

            return;
        }

        if (VoyageManager.Instance == null)
        {
            Debug.LogError(
                "VoyageCrewManager: VoyageManager.Instance is NULL."
            );

            return;
        }

        string shipId =
            SceneNavigator.selectedShipId;

        if (string.IsNullOrEmpty(shipId))
        {
            Debug.LogError(
                "VoyageCrewManager: No selected ship ID."
            );

            return;
        }

        VoyageData voyage =
            VoyageManager.Instance.GetVoyageByShipId(shipId);

        if (voyage == null)
        {
            Debug.LogError(
                "VoyageCrewManager: No voyage found for ship " +
                shipId
            );

            return;
        }

        if (voyage.crew == null ||
            voyage.crew.Count == 0)
        {
            Debug.LogWarning(
                "VoyageCrewManager: Voyage contains no crew."
            );

            return;
        }

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "BUILDING VOYAGE CREW"
        );

        Debug.Log(
            "Ship: " +
            shipId
        );

        Debug.Log(
            "Crew Count: " +
            voyage.crew.Count
        );

        Debug.Log(
            "================================"
        );


        int sailorIndex = 0;


        foreach (
            VoyageCrewData crewData
            in voyage.crew
        )
        {
            if (crewData == null)
            {
                continue;
            }

            Transform station =
                GetStationForRole(
                    crewData.shipRole,
                    ref sailorIndex
                );

            if (station == null)
            {
                Debug.LogWarning(
                    "NO STATION AVAILABLE | " +
                    crewData.crewName +
                    " | Role: " +
                    crewData.shipRole
                );

                continue;
            }

            SpawnCrewMember(
                crewData,
                station
            );
        }


        Debug.Log(
            "VOYAGE CREW CREATED: " +
            spawnedCrew.Count
        );

        Debug.Log(
            "================================"
        );
    }


    // =========================================================
    // GET STATION
    // =========================================================

    private Transform GetStationForRole(
        VoyageRole role,
        ref int sailorIndex
    )
    {
        switch (role)
        {
            case VoyageRole.Captain:

                return captainPosition;


            case VoyageRole.Lookout:

                return lookoutPosition;


            case VoyageRole.Sailor:

                if (sailorIndex == 0)
                {
                    sailorIndex++;
                    return sailPosition1;
                }

                if (sailorIndex == 1)
                {
                    sailorIndex++;
                    return sailPosition2;
                }

                if (sailorIndex == 2)
                {
                    sailorIndex++;
                    return sailPosition3;
                }

                break;
        }

        return null;
    }


    // =========================================================
    // CREATE VOYAGE CREW
    // =========================================================

    private void SpawnCrewMember(
        VoyageCrewData data,
        Transform station
    )
    {
        if (station == null)
        {
            Debug.LogWarning(
                "Cannot spawn crew. Station is missing."
            );

            return;
        }


        GameObject crewObject =
            Instantiate(
                crewPrefab,
                station.position,
                station.rotation,
                transform
            );


        VoyageCrew voyageCrew =
            crewObject.GetComponent<VoyageCrew>();


        if (voyageCrew == null)
        {
            Debug.LogError(
                "Voyage crew prefab is missing " +
                "VoyageCrew component."
            );

            Destroy(crewObject);

            return;
        }


        voyageCrew.crewId =
            data.crewId;

        voyageCrew.crewName =
            data.crewName;

        voyageCrew.role =
            data.shipRole;

        voyageCrew.targetPosition =
            station;


        crewObject.transform.position =
            station.position;

        crewObject.transform.rotation =
            station.rotation;


        spawnedCrew.Add(
            crewObject
        );


        Debug.Log(
            "VOYAGE CREW SPAWNED | " +
            data.crewName +
            " | ID: " +
            data.crewId +
            " | Role: " +
            data.shipRole +
            " | Station: " +
            station.name
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    public void ClearCrew()
    {
        foreach (
            GameObject crew
            in spawnedCrew
        )
        {
            if (crew != null)
            {
                Destroy(crew);
            }
        }

        spawnedCrew.Clear();
    }
}