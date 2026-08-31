using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance;

    public static string selectedShipId;

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

    public void GoToVoyage(
        string shipId
    )
    {
        if (
            ShipManager.Instance == null
        )
        {
            Debug.LogError(
                "Cannot open Voyage Scene. " +
                "ShipManager not found."
            );

            return;
        }

        ShipState ship =
            ShipManager.Instance.GetShip(
                shipId
            );

        if (ship == null)
        {
            Debug.LogError(
                "Cannot open Voyage Scene. " +
                "Ship not found: " +
                shipId
            );

            return;
        }

        if (!ship.onVoyage)
        {
            Debug.Log(
                "Cannot open Voyage Scene. " +
                ship.shipName +
                " is not currently on a voyage."
            );

            return;
        }

        selectedShipId =
            ship.shipId;

        Debug.Log(
            "OPENING VOYAGE VIEW: " +
            ship.shipName +
            " | ID: " +
            ship.shipId
        );

        if (
            ship.voyagePhase ==
            VoyagePhase.LeavingIsland
        )
        {
            Debug.Log(
                "Cannot open Voyage Scene. " +
                ship.shipName +
                " is still leaving the island."
            );

            return;
        }

        SceneManager.sceneLoaded +=
            OnVoyageSceneLoaded;

        SceneManager.LoadScene(
            "VoyageScene"
        );
    }

    private void OnVoyageSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        SceneManager.sceneLoaded -=
            OnVoyageSceneLoaded;

        if (
            string.IsNullOrEmpty(
                selectedShipId
            )
        )
        {
            Debug.LogWarning(
                "Voyage Scene loaded without " +
                "a selected ship."
            );

            return;
        }

        if (
            VoyageManager.Instance == null
        )
        {
            Debug.LogError(
                "VoyageManager not found " +
                "after loading VoyageScene."
            );

            return;
        }

        VoyageManager.Instance
            .RebuildSelectedVoyage();

        Debug.Log(
            "VOYAGE SCENE LOADED - " +
            "SELECTED SHIP RESTORED: " +
            selectedShipId
        );
    }

    public void GoToIsland()
    {
        Debug.Log(
            "RETURNING TO ISLAND SCENE"
        );

        SceneManager.sceneLoaded +=
            OnIslandSceneLoaded;

        SceneManager.LoadScene(
            "IslandScene"
        );
    }

    private void OnIslandSceneLoaded(
    Scene scene,
    LoadSceneMode mode
)
    {
        SceneManager.sceneLoaded -=
            OnIslandSceneLoaded;

        if (scene.name != "IslandScene")
        {
            return;
        }

        Debug.Log(
            "ISLAND SCENE LOADED - " +
            "RESTORING CREW VISIBILITY"
        );

        Invoke(
            nameof(RestoreCrewAfterIslandLoad),
            0.1f
        );
    }

    private void RestoreCrewAfterIslandLoad()
    {
        if (CrewManager.Instance == null)
        {
            Debug.LogWarning(
                "Cannot restore crew: CrewManager missing."
            );

            return;
        }

        CrewManager.Instance
            .RestoreIslandCrewVisibility();
    }

    private void RestoreIslandCrewVisibility()
    {
        if (CrewManager.Instance == null)
        {
            Debug.LogError(
                "CANNOT RESTORE CREW - CrewManager missing"
            );

            return;
        }

        List<CrewMovement> crew =
            CrewManager.Instance.crewMembers;

        List<CrewData> data =
            CrewManager.Instance.crewData;

        Debug.Log(
            "===== RESTORING ISLAND CREW ===== | Runtime Crew: " +
            crew.Count +
            " | Saved Crew: " +
            data.Count
        );

        foreach (CrewMovement crewMember in crew)
        {
            if (crewMember == null)
                continue;

            if (crewMember.crewData == null)
            {
                Debug.LogWarning(
                    "CREW HAS NO DATA | GO: " +
                    crewMember.gameObject.name
                );

                continue;
            }

            string crewId =
                crewMember.crewData.crewId;

            if (string.IsNullOrEmpty(crewId))
            {
                Debug.LogWarning(
                    "CREW HAS NO ID | GO: " +
                    crewMember.gameObject.name
                );

                continue;
            }

            CrewData savedData = null;

            foreach (CrewData d in data)
            {
                if (d == null)
                    continue;

                if (d.crewId == crewId)
                {
                    savedData = d;
                    break;
                }
            }

            if (savedData == null)
            {
                Debug.LogWarning(
                    "NO SAVED DATA FOUND | Crew ID: " +
                    crewId
                );

                continue;
            }

            // Make sure the scene crew uses the persistent data
            crewMember.crewData = savedData;

            if (savedData.isOnVoyage)
            {
                crewMember.HideIslandRepresentation();

                Debug.Log(
                    "HIDING CREW | " +
                    savedData.crewName +
                    " | ID: " +
                    savedData.crewId +
                    " | ON VOYAGE"
                );
            }
            else
            {
                crewMember.ShowIslandRepresentation();

                Debug.Log(
                    "SHOWING CREW | " +
                    savedData.crewName +
                    " | ID: " +
                    savedData.crewId +
                    " | ON ISLAND"
                );
            }
        }
    }
}