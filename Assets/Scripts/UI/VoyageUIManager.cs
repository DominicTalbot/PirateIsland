using TMPro;
using UnityEngine;

public class VoyageUIManager : MonoBehaviour
{
    public static VoyageUIManager Instance;

    [Header("Voyage Status")]
    public TextMeshProUGUI voyagePhaseText;

    public TextMeshProUGUI voyageDestinationText;

    [Header("Discovery UI")]
    public GameObject discoveryPanel;

    public TextMeshProUGUI eventTypeText;

    public TextMeshProUGUI discoveryTitle;

    public TextMeshProUGUI eventDescription;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateVoyageStatus();
    }

    private void UpdateVoyageStatus()
    {
        if (
            VoyageManager.Instance == null
        )
        {
            return;
        }

        VoyageData voyage =
            VoyageManager.Instance
                .GetActiveVoyage();

        if (voyage == null)
        {
            if (voyagePhaseText != null)
            {
                voyagePhaseText.text =
                    "NO ACTIVE VOYAGE";
            }

            if (voyageDestinationText != null)
            {
                voyageDestinationText.text =
                    "";
            }

            return;
        }

        if (voyagePhaseText != null)
        {
            voyagePhaseText.text =
                GetPhaseDisplayName(
                    voyage.voyagePhase
                );
        }

        if (voyageDestinationText != null)
        {
            voyageDestinationText.text =
                "DESTINATION\n" +
                voyage.voyageName;
        }
    }

    private string GetPhaseDisplayName(
        VoyagePhase phase
    )
    {
        switch (phase)
        {
            case VoyagePhase.LeavingIsland:
                return "LEAVING ISLAND";

            case VoyagePhase.TravellingToDestination:
                return "TRAVELLING TO DESTINATION";

            case VoyagePhase.ApproachingDestination:
                return "APPROACHING DESTINATION";

            case VoyagePhase.Mission:
                return "MISSION IN PROGRESS";

            case VoyagePhase.ReturningHome:
                return "RETURNING HOME";

            case VoyagePhase.ApproachingHome:
                return "APPROACHING ISLAND";

            case VoyagePhase.Complete:
                return "VOYAGE COMPLETE";

            default:
                return phase.ToString().ToUpper();
        }
    }

    public void ShowDiscovery(
        string title,
        string description
    )
    {
        if (discoveryPanel == null)
        {
            Debug.LogError(
                "Discovery Panel is not assigned."
            );

            return;
        }

        discoveryPanel.SetActive(true);

        if (eventTypeText != null)
        {
            eventTypeText.text =
                "LOOKOUT REPORT";
        }

        if (discoveryTitle != null)
        {
            discoveryTitle.text =
                title;
        }

        if (eventDescription != null)
        {
            eventDescription.text =
                description;
        }
    }

    public void Investigate()
    {
        Debug.Log(
            "PLAYER CHOSE: INVESTIGATE"
        );

        if (discoveryPanel != null)
        {
            discoveryPanel.SetActive(false);
        }

        if (
            VoyageEventManager.Instance != null
        )
        {
            VoyageEventManager.Instance
                .ResolveEvent(true);
        }
        else
        {
            Debug.LogError(
                "VoyageEventManager.Instance is NULL!"
            );
        }
    }

    public void Ignore()
    {
        Debug.Log(
            "PLAYER CHOSE: IGNORE"
        );

        if (discoveryPanel != null)
        {
            discoveryPanel.SetActive(false);
        }

        if (
            VoyageEventManager.Instance != null
        )
        {
            VoyageEventManager.Instance
                .ResolveEvent(false);
        }
        else
        {
            Debug.LogError(
                "VoyageEventManager.Instance is NULL!"
            );
        }
    }
}