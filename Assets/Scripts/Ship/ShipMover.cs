using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipMover : MonoBehaviour
{
    [Header("Dock Rotation")]
    public Vector3 dockRotation = new Vector3(0f, 92.509f, 0f);
    public Transform dockPoint;

    [Header("Exit Points")]
    public Transform northExit;
    public Transform southExit;
    public Transform eastExit;
    public Transform westExit;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 2f;

    [Header("Bobbing")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.08f;

    [Header("Departure Fade")]
    public float fadeDuration = 2f;

    private bool missionActive;
    private float baseY;

    private Transform currentExitPoint;

    private string shipId;
    private ShipState shipState;

    private Renderer[] shipRenderers;
    private bool departureFadeRunning;

    private void Start()
    {
        baseY = transform.position.y;

        shipRenderers = GetComponentsInChildren<Renderer>(true);

        StartCoroutine(SetupShipAfterManagersReady());
    }

    private System.Collections.IEnumerator SetupShipAfterManagersReady()
    {
        yield return null;

        ConnectToShip();

        if (SceneManager.GetActiveScene().name == "IslandScene")
        {
            SetupIslandShip();
        }
    }

    private void Update()
    {
        ApplyBobbing();

        if (SceneManager.GetActiveScene().name == "VoyageScene")
        {
            return;
        }

        if (!missionActive)
        {
            Quaternion targetRotation =
                Quaternion.Euler(dockRotation);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 2f
                );

            return;
        }

        if (currentExitPoint == null)
        {
            Debug.LogError("Ship has no exit point.");

            missionActive = false;
            return;
        }

        MoveTowardsTarget(currentExitPoint.position);

        float distance =
            Vector3.Distance(
                transform.position,
                currentExitPoint.position
            );

        if (distance < 0.5f)
        {
            HandleIslandDeparture();
        }
    }

    // =========================================================
    // ISLAND SHIP SETUP
    // =========================================================

    private void SetupIslandShip()
    {
        if (shipState == null)
        {
            Debug.LogWarning(
                "Island ship setup waiting for ShipState."
            );

            return;
        }

        /*
         * IMPORTANT:
         *
         * Never disable the entire Ship GameObject.
         * ShipMover must remain alive so the ship can
         * be restored after the voyage.
         */

        if (shipState.onVoyage)
        {
            HideShip();

            Debug.Log(
                "SHIP IS CURRENTLY AWAY: " +
                shipState.shipName
            );

            return;
        }

        RestoreShipAtDock();
    }

    // =========================================================
    // DEPARTURE
    // =========================================================

    public void StartJourney()
    {
        ConnectToShip();

        if (shipState == null)
        {
            Debug.LogError(
                "Cannot start journey. ShipState is missing."
            );

            return;
        }

        missionActive = true;

        ShowShip();

        SetVoyagePhase(
            VoyagePhase.LeavingIsland
        );

        ChooseExitPoint();

        Debug.Log(
            "SHIP JOURNEY STARTED: " +
            shipState.shipName +
            " | Destination: " +
            shipState.destinationName
        );
    }

    private void HandleIslandDeparture()
    {
        if (shipState == null)
        {
            Debug.LogError(
                "Cannot complete departure. ShipState is missing."
            );

            return;
        }

        missionActive = false;

        shipState.onVoyage = true;

        // This changes the phase AND starts the persistent
        // outbound voyage timer inside VoyageManager.
        SetVoyagePhase(
            VoyagePhase.TravellingToDestination
        );

        shipState.worldX =
            transform.position.x;

        shipState.worldZ =
            transform.position.z;

        Debug.Log(
            "SHIP LEFT ISLAND: " +
            shipState.shipName
        );

        StartCoroutine(
            FadeShipAndLeave()
        );
    }

    // =========================================================
    // FADE
    // =========================================================

    private System.Collections.IEnumerator FadeShipAndLeave()
    {
        if (departureFadeRunning)
        {
            yield break;
        }

        departureFadeRunning = true;

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        Material[] materials =
            new Material[renderers.Length];

        Color[] originalColors =
            new Color[renderers.Length];

        for (
            int i = 0;
            i < renderers.Length;
            i++
        )
        {
            materials[i] =
                renderers[i].material;

            if (
                materials[i].HasProperty("_BaseColor")
            )
            {
                originalColors[i] =
                    materials[i].GetColor(
                        "_BaseColor"
                    );
            }
            else if (
                materials[i].HasProperty("_Color")
            )
            {
                originalColors[i] =
                    materials[i].GetColor(
                        "_Color"
                    );
            }
            else
            {
                originalColors[i] =
                    Color.white;
            }
        }

        float timer = 0f;

        while (
            timer < fadeDuration
        )
        {
            timer +=
                Time.deltaTime;

            float alpha =
                Mathf.Clamp01(
                    1f -
                    (
                        timer /
                        fadeDuration
                    )
                );

            for (
                int i = 0;
                i < materials.Length;
                i++
            )
            {
                Color color =
                    originalColors[i];

                color.a =
                    alpha;

                if (
                    materials[i].HasProperty(
                        "_BaseColor"
                    )
                )
                {
                    materials[i].SetColor(
                        "_BaseColor",
                        color
                    );
                }
                else if (
                    materials[i].HasProperty(
                        "_Color"
                    )
                )
                {
                    materials[i].SetColor(
                        "_Color",
                        color
                    );
                }
            }

            yield return null;
        }

        /*
         * IMPORTANT:
         *
         * Do NOT disable the GameObject.
         *
         * Only hide its renderers.
         */

        HideShip();

        departureFadeRunning = false;

        Debug.Log(
            "SHIP HIDDEN FROM ISLAND - " +
            "VOYAGE CONTINUES"
        );
    }

    // =========================================================
    // RETURN COMPATIBILITY
    // =========================================================

    public void BeginReturnJourney()
    {
        Debug.Log(
            "SHIP RETURN JOURNEY REQUESTED"
        );

        if (shipState == null)
        {
            ConnectToShip();
        }

        if (shipState == null)
        {
            Debug.LogError(
                "Cannot begin return journey. " +
                "ShipState is missing."
            );

            return;
        }

        SetVoyagePhase(
            VoyagePhase.ReturningHome
        );
    }

    // =========================================================
    // RESTORE SHIP
    // =========================================================

    public void RestoreShipAtDock()
    {
        if (shipState == null)
        {
            ConnectToShip();
        }

        if (shipState == null)
        {
            Debug.LogError(
                "RestoreShipAtDock: ShipState missing."
            );

            return;
        }

        Debug.Log(
            "RESTORING SHIP AT ISLAND | " +
            shipState.shipName
        );

        missionActive = false;
        currentExitPoint = null;

        if (dockPoint == null)
        {
            Debug.LogError(
                "RESTORE FAILED: ShipMover dockPoint " +
                "is NOT ASSIGNED."
            );

            return;
        }

        transform.position =
            dockPoint.position;

        transform.rotation =
            Quaternion.Euler(
                dockRotation
            );

        baseY =
            transform.position.y;

        ShowShip();

        shipState.worldX =
            transform.position.x;

        shipState.worldZ =
            transform.position.z;

        shipState.onVoyage = false;

        shipState.voyageProgress = 100f;

        shipState.voyagePhase =
            VoyagePhase.Complete;

        Debug.Log(
            "SHIP RESTORED AT ISLAND DOCK: " +
            shipState.shipName +
            " | Position: " +
            transform.position
        );
    }

    // =========================================================
    // VISIBILITY
    // =========================================================

    private void HideShip()
    {
        if (shipRenderers == null)
        {
            shipRenderers =
                GetComponentsInChildren<Renderer>(true);
        }

        foreach (
            Renderer renderer
            in shipRenderers
        )
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    private void ShowShip()
    {
        if (shipRenderers == null)
        {
            shipRenderers =
                GetComponentsInChildren<Renderer>(true);
        }

        foreach (
            Renderer renderer
            in shipRenderers
        )
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        ResetShipAppearance();
    }

    // =========================================================
    // EXIT SELECTION
    // =========================================================

    private void ChooseExitPoint()
    {
        if (dockPoint == null)
        {
            Debug.LogError(
                "Dock point is missing."
            );

            return;
        }

        if (MissionManager.Instance == null)
        {
            Debug.LogError(
                "MissionManager not found."
            );

            return;
        }

        if (
            MissionManager.Instance.currentMission == null
        )
        {
            Debug.LogError(
                "Current mission is NULL."
            );

            return;
        }

        Transform missionTarget =
            MissionManager.Instance
                .currentMission
                .destinationPoint;

        if (missionTarget == null)
        {
            Debug.LogError(
                "Mission destination missing."
            );

            return;
        }

        Vector3 direction =
            missionTarget.position -
            dockPoint.position;

        direction.y = 0f;

        Debug.Log(
            "Choosing island exit. Direction: " +
            direction
        );

        if (
            Mathf.Abs(direction.x) >
            Mathf.Abs(direction.z)
        )
        {
            if (direction.x > 0f)
            {
                currentExitPoint =
                    eastExit;

                Debug.Log(
                    "Using EAST exit."
                );
            }
            else
            {
                currentExitPoint =
                    westExit;

                Debug.Log(
                    "Using WEST exit."
                );
            }
        }
        else
        {
            if (direction.z > 0f)
            {
                currentExitPoint =
                    northExit;

                Debug.Log(
                    "Using NORTH exit."
                );
            }
            else
            {
                currentExitPoint =
                    southExit;

                Debug.Log(
                    "Using SOUTH exit."
                );
            }
        }

        if (currentExitPoint == null)
        {
            Debug.LogError(
                "Current Exit Point is NULL."
            );

            return;
        }

        Debug.Log(
            "Exit Point Assigned: " +
            currentExitPoint.name
        );
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveTowardsTarget(
        Vector3 target
    )
    {
        Vector3 direction =
            target -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // SHIP CONNECTION
    // =========================================================

    private void ConnectToShip()
    {
        if (ShipManager.Instance == null)
        {
            Debug.LogError(
                "ShipManager not found."
            );

            return;
        }

        shipId =
            SceneNavigator.selectedShipId;

        if (string.IsNullOrEmpty(shipId))
        {
            if (VoyageManager.Instance != null)
            {
                VoyageData voyage =
                    VoyageManager.Instance
                        .GetActiveVoyage();

                if (voyage != null)
                {
                    shipId =
                        voyage.shipId;
                }
            }
        }

        if (string.IsNullOrEmpty(shipId))
        {
            Debug.LogWarning(
                "No ship ID available."
            );

            return;
        }

        shipState =
            ShipManager.Instance.GetShip(
                shipId
            );

        if (shipState == null)
        {
            Debug.LogError(
                "Could not find ShipState for: " +
                shipId
            );

            return;
        }

        Debug.Log(
            "SHIP MOVER CONNECTED: " +
            shipState.shipName +
            " | ID: " +
            shipState.shipId
        );
    }

    // =========================================================
    // VOYAGE PHASE
    // =========================================================

    private void SetVoyagePhase(
        VoyagePhase phase
    )
    {
        if (shipState == null)
        {
            return;
        }

        if (VoyageManager.Instance != null)
        {
            VoyageData voyage =
                VoyageManager.Instance
                    .GetVoyageByShipId(
                        shipState.shipId
                    );

            if (voyage != null)
            {
                VoyageManager.Instance.SetVoyagePhase(
                    voyage,
                    phase
                );

                return;
            }
        }

        shipState.voyagePhase = phase;
    }

    // =========================================================
    // SHIP APPEARANCE
    // =========================================================

    private void ResetShipAppearance()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (
            Renderer renderer
            in renderers
        )
        {
            if (renderer == null)
            {
                continue;
            }

            Material material =
                renderer.material;

            if (
                material.HasProperty(
                    "_BaseColor"
                )
            )
            {
                Color color =
                    material.GetColor(
                        "_BaseColor"
                    );

                color.a = 1f;

                material.SetColor(
                    "_BaseColor",
                    color
                );
            }
            else if (
                material.HasProperty(
                    "_Color"
                )
            )
            {
                Color color =
                    material.GetColor(
                        "_Color"
                    );

                color.a = 1f;

                material.SetColor(
                    "_Color",
                    color
                );
            }
        }
    }

    // =========================================================
    // BOBBING
    // =========================================================

    private void ApplyBobbing()
    {
        float bobOffset =
            Mathf.Sin(
                Time.time *
                bobSpeed
            ) *
            bobHeight;

        Vector3 pos =
            transform.position;

        pos.y =
            baseY +
            bobOffset;

        transform.position =
            pos;
    }
}