using UnityEngine;

public class VoyageEventManager : MonoBehaviour
{
    public static VoyageEventManager Instance;

    [Header("Event Settings")]
    public float firstEventDelay = 15f;
    public float eventInterval = 30f;

    [Header("Investigation")]
    public float investigationDuration = 10f;

    [Header("UI")]
    public VoyageUIManager voyageUIManager;

    private float eventTimer;
    private float investigationTimer;

    private bool eventActive;
    private bool investigating;

    private ShipState viewedShip;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        enabled = false;
        return;
        ConnectToViewedShip();

        if (viewedShip == null)
        {
            Debug.Log(
                "VOYAGE EVENTS DISABLED - " +
                "No ship is being viewed."
            );

            enabled = false;

            return;
        }

        eventTimer =
            firstEventDelay;

        Debug.Log(
            "VOYAGE EVENTS ACTIVE FOR: " +
            viewedShip.shipName +
            " | ID: " +
            viewedShip.shipId
        );
    }

    private void Update()
    {
        if (viewedShip == null)
        {
            return;
        }

        if (!viewedShip.onVoyage)
        {
            return;
        }

        if (investigating)
        {
            investigationTimer -=
                Time.deltaTime;

            if (
                investigationTimer <= 0f
            )
            {
                FinishInvestigation();
            }

            return;
        }

        if (eventActive)
        {
            return;
        }

        eventTimer -=
            Time.deltaTime;

        if (
            eventTimer <= 0f
        )
        {
            TriggerWreckage();

            eventTimer =
                eventInterval;
        }
    }

    private void ConnectToViewedShip()
    {
        if (
            ShipManager.Instance == null
        )
        {
            Debug.LogError(
                "ShipManager not found."
            );

            return;
        }

        if (
            string.IsNullOrEmpty(
                SceneNavigator.selectedShipId
            )
        )
        {
            Debug.Log(
                "No selected ship for Voyage Events."
            );

            return;
        }

        viewedShip =
            ShipManager.Instance.GetShip(
                SceneNavigator.selectedShipId
            );

        if (viewedShip == null)
        {
            Debug.LogError(
                "Could not find viewed ship: " +
                SceneNavigator.selectedShipId
            );

            return;
        }

        if (!viewedShip.onVoyage)
        {
            Debug.Log(
                "Viewed ship is not currently " +
                "on a voyage."
            );

            viewedShip = null;

            return;
        }
    }

    private void TriggerWreckage()
    {
        if (viewedShip == null)
        {
            return;
        }

        if (!viewedShip.onVoyage)
        {
            return;
        }

        eventActive = true;

        Debug.Log(
            "VOYAGE EVENT: Wreckage spotted!"
        );

        if (voyageUIManager != null)
        {
            voyageUIManager.ShowDiscovery(
                "WRECKAGE SPOTTED",
                "The lookout has spotted wreckage floating nearby."
            );
        }
        else
        {
            Debug.LogError(
                "VoyageUIManager is not assigned!"
            );
        }
    }

    public void ResolveEvent(
        bool investigate
    )
    {
        if (viewedShip == null)
        {
            Debug.LogWarning(
                "Cannot resolve event. " +
                "No ship is being viewed."
            );

            return;
        }

        Debug.Log(
            "ResolveEvent called. Investigate = " +
            investigate
        );

        if (!investigate)
        {
            Debug.Log(
                "EVENT IGNORED - VOYAGE CONTINUES"
            );

            eventActive = false;

            return;
        }

        Debug.Log(
            "EVENT INVESTIGATED"
        );

        eventActive = false;

        investigating = true;

        BeginInvestigation();
    }

    private void BeginInvestigation()
    {
        Debug.Log(
            "INVESTIGATION STARTED"
        );

        investigationTimer =
            investigationDuration;
    }

    private void FinishInvestigation()
    {
        investigating = false;

        if (viewedShip == null)
        {
            Debug.LogWarning(
                "Investigation finished, " +
                "but no ship is being viewed."
            );

            return;
        }

        if (!viewedShip.onVoyage)
        {
            Debug.LogWarning(
                "Investigation finished, " +
                "but ship is no longer on voyage."
            );

            return;
        }

        if (
            VoyageManager.Instance == null
        )
        {
            Debug.LogError(
                "VoyageManager not found."
            );

            return;
        }

        VoyageData voyage =
            VoyageManager.Instance
                .GetVoyageByShipId(
                    viewedShip.shipId
                );

        if (voyage == null)
        {
            Debug.LogError(
                "Could not find voyage data for ship: " +
                viewedShip.shipId
            );

            return;
        }

        bool added =
            VoyageManager.Instance
                .AddCargo(
                    voyage,
                    CargoType.Materials,
                    5
                );

        if (added)
        {
            Debug.Log(
                "WRECKAGE REWARD: " +
                "5 MATERIALS"
            );
        }
        else
        {
            Debug.Log(
                "WRECKAGE REWARD FAILED: " +
                "Cargo hold is full."
            );
        }

        VoyageManager.Instance
            .PrintCargo(
                voyage
            );
    }
}