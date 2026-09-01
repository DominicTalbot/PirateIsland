using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main Panels")]

    public GameObject buildingPanel;

    public GameObject missionsPanel;

    public GameObject shipyardPanel;

    [Header("HUD")]

    public TextMeshProUGUI goldText;

    public TextMeshProUGUI suppliesText;

    public TextMeshProUGUI crewText;

    public TextMeshProUGUI moraleText;

    public TextMeshProUGUI missionStatusText;

    public TextMeshProUGUI missionTimerText;

    public TextMeshProUGUI jobOverviewText;

    public TextMeshProUGUI debugCrewText;

    [Header("Fishing UI")]

    public TextMeshProUGUI fishingCrewText;

    [Header("Building UI")]

    public TextMeshProUGUI buildingTitle;

    public TextMeshProUGUI upgradeProgressText;

    public Slider upgradeProgressSlider;

    public TextMeshProUGUI builderCrewText;

    public TextMeshProUGUI upgradeCostText;

    [Header("Building Sections")]

    public GameObject docksSection;

    public GameObject storageSection;

    public GameObject tavernSection;

    public GameObject barracksSection;

    public GameObject mainBuildingSection;

    [Header("Building Info Text")]

    public TextMeshProUGUI storageInfoText;

    public TextMeshProUGUI tavernInfoText;

    public TextMeshProUGUI barracksInfoText;

    public TextMeshProUGUI mainBuildingInfoText;

    public TextMeshProUGUI docksInfoText;

    [Header("Mission UI")]

    public TextMeshProUGUI missionInfoText;

    public TextMeshProUGUI missionCrewText;

    public TextMeshProUGUI successChanceText;

    [Header("Mission Buttons")]

    public Image nearbyWreckImage;

    public Image merchantShipImage;

    public Image navyPatrolImage;

    [Header("Mission Colors")]

    public Color selectedColor =
        Color.yellow;

    public Color normalColor =
        Color.white;

    [Header("Mission Controls")]

    public Button startMissionButton;

    private BuildingClickable currentBuilding;

    public Button addCrewButton;

    public Button removeCrewButton;

    [Header("Fishing Controls")]

    [SerializeField] private Button removeFishingButton;
    [SerializeField] private Button addFishingButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupFishingButtons();
        BindMissionButtons();
        BindFishingButtons();
    }

    private void SetupFishingButtons()
    {
        if (removeFishingButton != null)
        {
            removeFishingButton.onClick.RemoveAllListeners();
            removeFishingButton.onClick.AddListener(
                CrewManager.Instance.RemoveFishingCrew
            );
        }

        if (addFishingButton != null)
        {
            addFishingButton.onClick.RemoveAllListeners();
            addFishingButton.onClick.AddListener(
                CrewManager.Instance.AddFishingCrew
            );
        }
    }

    private void BindMissionButtons()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogWarning(
                "MissionManager not found."
            );

            return;
        }

        Button nearbyButton =
            nearbyWreckImage.GetComponent<Button>();

        Button merchantButton =
            merchantShipImage.GetComponent<Button>();

        Button navyButton =
            navyPatrolImage.GetComponent<Button>();

        if (nearbyButton != null)
        {
            nearbyButton.onClick.AddListener(
                MissionManager.Instance.SelectNearbyWreck
            );
        }

        if (merchantButton != null)
        {
            merchantButton.onClick.AddListener(
                MissionManager.Instance.SelectMerchantShip
            );
        }

        if (navyButton != null)
        {
            navyButton.onClick.AddListener(
                MissionManager.Instance.SelectNavyPatrol
            );
        }

        if (addCrewButton != null)
        {
            addCrewButton.onClick.AddListener(
                MissionManager.Instance.AddCrew
            );
        }

        if (removeCrewButton != null)
        {
            removeCrewButton.onClick.AddListener(
                MissionManager.Instance.RemoveCrew
            );
        }

        if (startMissionButton != null)
        {
            startMissionButton.onClick.AddListener(
                MissionManager.Instance.StartMission
            );
        }
    }

    private void BindFishingButtons()
    {
        if (CrewManager.Instance == null)
        {
            Debug.LogWarning(
                "FISHING BUTTON BIND FAILED | CrewManager not ready."
            );

            return;
        }

        if (addFishingButton != null)
        {
            addFishingButton.onClick.RemoveAllListeners();

            addFishingButton.onClick.AddListener(
                CrewManager.Instance.AddFishingCrew
            );

            Debug.Log(
                "FISHING + BUTTON BOUND TO CURRENT CREWMANAGER"
            );
        }

        if (removeFishingButton != null)
        {
            removeFishingButton.onClick.RemoveAllListeners();

            removeFishingButton.onClick.AddListener(
                CrewManager.Instance.RemoveFishingCrew
            );

            Debug.Log(
                "FISHING - BUTTON BOUND TO CURRENT CREWMANAGER"
            );
        }
    }

    private void Update()
    {
        UpdateHUD();

        UpdateBuildingProgress();
    }

    private void UpdateHUD()
    {
        // =========================================================
        // SAFETY CHECKS
        // =========================================================

        if (GameManager.Instance == null)
        {
            return;
        }

        if (CrewManager.Instance == null)
        {
            return;
        }


        // =========================================================
        // MAIN HUD
        // =========================================================

        if (goldText != null)
        {
            goldText.text =
                "Gold: " +
                GameManager.Instance.gold +
                "/" +
                GameManager.Instance.goldStorage;
        }


        if (crewText != null)
        {
            crewText.text =
                "Crew: " +
                GameManager.Instance.availableCrew +
                "/" +
                GameManager.Instance.maxCrew;
        }


        if (moraleText != null)
        {
            moraleText.text =
                "Morale: " +
                GameManager.Instance.morale;
        }


        if (suppliesText != null)
        {
            suppliesText.text =
                "Supplies: " +
                GameManager.Instance.supplies +
                "/" +
                GameManager.Instance.maxSupplies;
        }


        // =========================================================
        // FISHING
        // =========================================================

        if (fishingCrewText != null)
        {
            fishingCrewText.text =
                "Fishing Crew: " +
                CrewManager.Instance.fishingAssigned;
        }


        // =========================================================
        // BUILDERS
        // =========================================================

        if (builderCrewText != null)
        {
            if (
                CrewManager.Instance.activeConstruction == null
            )
            {
                builderCrewText.text =
                    "No Construction";
            }
            else
            {
                builderCrewText.text =
                    "Builders: " +
                    CrewManager.Instance.builderAssigned;
            }
        }


        // =========================================================
        // JOB OVERVIEW
        // =========================================================

        if (jobOverviewText != null)
        {
            jobOverviewText.text =
                "Idle Crew: " +
                GameManager.Instance.availableCrew +
                "\nFishing Crew: " +
                CrewManager.Instance.fishingCrew +
                "\nBuilders: " +
                CrewManager.Instance.builderCrew;
        }


        // =========================================================
        // DEBUG CREW
        // =========================================================

        if (debugCrewText != null)
        {
            debugCrewText.text =
                CrewManager.Instance.GetCrewDebugText();
        }
    }

    public void OpenBuildingPanel(
        BuildingClickable building
    )
    {
        currentBuilding = building;

        buildingPanel.SetActive(true);

        HideAllSections();

        if (building.buildingName == "Docks")
        {
            upgradeCostText.gameObject.SetActive(false);
        }
        else
        {
            upgradeCostText.gameObject.SetActive(true);

            upgradeCostText.text =
                "Upgrade Cost: " +
                building.upgradeCost +
                " Gold";
        }

        buildingTitle.text =
            building.buildingName +
            " - Level " +
            building.buildingLevel;

        switch (building.buildingName)
        {
            case "Docks":

                docksSection.SetActive(true);

                docksInfoText.text =
                    "Upgrade Cost: " +
                    building.upgradeCost +
                    " Gold" +

                    "\n\nCurrent Level: " +
                    building.buildingLevel +

                    "\n\nNext Upgrade:" +

                    "\nUnlock Better Ships";

                break;

            case "Storage":

                storageSection.SetActive(true);

                storageInfoText.text =
                    "Gold Capacity: " +

                    GameManager.Instance.goldStorage +

                    "\nSupply Capacity: " +

                    GameManager.Instance.maxSupplies;

                if (
                    GameManager.Instance
                    .expandedWarehouseUnlocked
                )
                {
                    storageInfoText.text +=
                        "\n• Expanded Warehouses";
                }

                break;

            case "Tavern":

                tavernSection.SetActive(true);

                tavernInfoText.text =
                    "Morale: " +
                    GameManager.Instance.morale;

                if (
                    GameManager.Instance
                    .liveMusicUnlocked
                )
                {
                    tavernInfoText.text +=
                        "\n• Live Music Active";
                }

                break;

            case "Barracks":

                barracksSection.SetActive(true);

                barracksInfoText.text =
                    "Max Crew: " +
                    GameManager.Instance.maxCrew;

                break;

            case "Main Building":

                mainBuildingSection.SetActive(true);

                mainBuildingInfoText.text =
                    "Island Tier: " +
                    building.buildingLevel;

                break;
        }
    }

    private void HideAllSections()
    {
        docksSection.SetActive(false);

        storageSection.SetActive(false);

        tavernSection.SetActive(false);

        barracksSection.SetActive(false);

        mainBuildingSection.SetActive(false);
    }

    public void CloseBuildingPanel()
    {
        buildingPanel.SetActive(false);
    }

    public void OpenMissionsPanel()
    {
        MissionManager.Instance.currentMission =
            null;

        buildingPanel.SetActive(false);

        missionsPanel.SetActive(true);

        HighlightMission(-1);

        UpdateMissionUI();
    }

    public void CloseMissionsPanel()
    {
        missionsPanel.SetActive(false);

        buildingPanel.SetActive(true);
    }

    public void OpenShipyardPanel()
    {
        buildingPanel.SetActive(false);

        shipyardPanel.SetActive(true);
    }

    public void CloseShipyardPanel()
    {
        shipyardPanel.SetActive(false);

        buildingPanel.SetActive(true);
    }

    public void UpgradeCurrentBuilding()
    {
        currentBuilding.StartUpgrade();

        buildingTitle.text =
            currentBuilding.buildingName +
            " - Level " +
            currentBuilding.buildingLevel;

        OpenBuildingPanel(currentBuilding);
    }

    public void UpdateMissionUI()
    {
        if (MissionManager.Instance == null)
        {
            return;
        }

        // ---------------------------------------------------------
        // Mission information
        // ---------------------------------------------------------

        if (
            MissionManager.Instance.currentMission != null
        )
        {
            MissionData mission =
                MissionManager.Instance.currentMission;

            if (missionInfoText != null)
            {
                missionInfoText.text =
                    mission.missionName +
                    "\nReward: " +
                    mission.reward +
                    "\nDuration: " +
                    mission.duration +
                    "s";
            }
        }
        else
        {
            if (missionInfoText != null)
            {
                missionInfoText.text =
                    "No Mission Selected";
            }
        }


        // ---------------------------------------------------------
        // Success chance
        // ---------------------------------------------------------

        int chance =
            MissionManager.Instance
            .GetSuccessChance();


        if (missionCrewText != null)
        {
            missionCrewText.text =
                "Crew Assigned: " +
                MissionManager.Instance.crewAssigned +

                "\nAvailable: " +

                (
                    GameManager.Instance != null
                        ? GameManager.Instance.availableCrew
                        : 0
                );
        }


        if (successChanceText != null)
        {
            successChanceText.text =
                "Success Chance: " +
                chance +
                "%";
        }


        // ---------------------------------------------------------
        // Start mission button
        //
        // The UI may have been destroyed when changing scenes.
        // Never try to access a destroyed button.
        // ---------------------------------------------------------

        if (startMissionButton != null)
        {
            startMissionButton.interactable =
                MissionManager.Instance.currentMission != null
                &&
                MissionManager.Instance.currentMission
                    .destinationPoint != null;
        }
    }

    public void UpdateMissionStatus(
        string status
    )
    {
        missionStatusText.text =
            status;
    }

    public void UpdateMissionTimer(
        float time
    )
    {
        missionTimerText.text =
            "Time Remaining: " +
            Mathf.Ceil(time) +
            "s";
    }

    public void HighlightMission(
        int missionIndex
    )
    {
        nearbyWreckImage.color =
            normalColor;

        merchantShipImage.color =
            normalColor;

        navyPatrolImage.color =
            normalColor;

        if (missionIndex < 0)
        {
            return;
        }

        switch (missionIndex)
        {
            case 0:

                nearbyWreckImage.color =
                    selectedColor;

                break;

            case 1:

                merchantShipImage.color =
                    selectedColor;

                break;

            case 2:

                navyPatrolImage.color =
                    selectedColor;

                break;
        }
    }

    void UpdateBuildingProgress()
    {
        if (currentBuilding == null)
        {
            upgradeProgressSlider.gameObject.SetActive(false);
            upgradeProgressText.text = "";

            return;
        }

        if (!currentBuilding.upgrading)
        {
            upgradeProgressSlider.gameObject.SetActive(false);
            upgradeProgressText.text = "";

            return;
        }

        upgradeProgressSlider.gameObject.SetActive(true);

        float progress =
            currentBuilding.GetUpgradeProgress();

        upgradeProgressSlider.value =
            progress;

        int percent =
            Mathf.RoundToInt(
                progress * 100f
            );

        upgradeProgressText.text =
            percent + "%";
    }
}
