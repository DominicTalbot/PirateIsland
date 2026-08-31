using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingClickable : MonoBehaviour
{
    [Header("Building Info")]

    public string buildingName;

    public int buildingLevel = 1;

    public int upgradeCost = 100;

    public Slider worldUpgradeSlider;

    public TextMeshProUGUI worldProgressText;

    public TextMeshProUGUI worldSpeedText;


    [Header("Construction")]

    public GameObject[] levelModels;

    public GameObject constructionVisual;

    public bool upgrading;

    public float upgradeTime = 20f;

    public BuildingLevelData[] levels;


    [Header("Builder")]

    public Transform builderSpot;


    // =========================================================
    // PERSISTENT STATE
    // =========================================================

    private BuildingPersistentState persistentState;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        LoadPersistentState();

        RefreshVisuals();

        RefreshConstructionVisuals();

        HideWorldProgress();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateFromPersistentState();
    }


    // =========================================================
    // LOAD PERSISTENT STATE
    // =========================================================

    private void LoadPersistentState()
    {
        if (
            GameManager.Instance == null
        )
        {
            Debug.LogError(
                "BUILDING ERROR | GameManager missing."
            );

            return;
        }


        persistentState =
            GameManager.Instance.GetBuildingState(
                buildingName
            );


        if (
            persistentState == null
        )
        {
            Debug.LogError(
                "BUILDING STATE NOT FOUND | " +
                buildingName
            );

            return;
        }


        /*
         * Restore everything from persistent state.
         */

        buildingLevel =
            persistentState.buildingLevel;

        upgrading =
            persistentState.upgrading;

        upgradeTime =
            persistentState.upgradeTime;

        upgradeCost =
            persistentState.upgradeCost;


        Debug.Log(
            "BUILDING STATE LOADED | " +
            buildingName +
            " | Level: " +
            buildingLevel +
            " | Upgrading: " +
            upgrading +
            " | Progress: " +
            (
                persistentState.GetProgress() * 100f
            ).ToString("F0") +
            "%"
        );
    }


    // =========================================================
    // UPDATE FROM PERSISTENT STATE
    // =========================================================

    private void UpdateFromPersistentState()
    {
        if (
            persistentState == null
        )
        {
            return;
        }


        /*
         * The GameManager owns the actual state.
         */

        buildingLevel =
            persistentState.buildingLevel;

        upgrading =
            persistentState.upgrading;

        upgradeTime =
            persistentState.upgradeTime;

        upgradeCost =
            persistentState.upgradeCost;


        /*
         * Update construction visuals.
         */

        RefreshConstructionVisuals();


        /*
         * Update world progress UI.
         */

        if (
            upgrading &&
            persistentState.constructionStarted
        )
        {
            ShowWorldProgress();

            float progress =
                persistentState.GetProgress();


            if (
                worldUpgradeSlider != null
            )
            {
                worldUpgradeSlider.value =
                    progress;
            }


            if (
                worldProgressText != null
            )
            {
                worldProgressText.text =
                    Mathf.RoundToInt(
                        progress * 100f
                    ) +
                    "%";
            }


            if (
                worldSpeedText != null &&
                CrewManager.Instance != null
            )
            {
                float buildSpeed =
                    1f +
                    Mathf.Sqrt(
                        CrewManager.Instance
                            .builderCrew
                    );


                worldSpeedText.text =
                    "Builders: " +
                    CrewManager.Instance
                        .builderCrew +
                    "\nSpeed: " +
                    buildSpeed.ToString("F1") +
                    "x";
            }
        }
        else
        {
            HideWorldProgress();
        }
    }


    // =========================================================
    // START UPGRADE
    // =========================================================

    public void StartUpgrade()
    {
        if (
            GameManager.Instance == null
        )
        {
            Debug.LogError(
                "Cannot upgrade building. " +
                "GameManager missing."
            );

            return;
        }


        if (
            CrewManager.Instance == null
        )
        {
            Debug.LogError(
                "Cannot upgrade building. " +
                "CrewManager missing."
            );

            return;
        }


        if (
            persistentState == null
        )
        {
            LoadPersistentState();
        }


        if (
            persistentState == null
        )
        {
            return;
        }


        /*
         * Only one building can be constructed
         * at a time.
         */

        if (
            CrewManager.Instance
                .activeConstruction != null
        )
        {
            if (
                CrewManager.Instance
                    .activeConstruction != this
            )
            {
                if (
                    UIManager.Instance != null
                )
                {
                    UIManager.Instance
                        .UpdateMissionStatus(
                            "CONSTRUCTION ALREADY ACTIVE"
                        );
                }

                return;
            }
        }


        if (
            persistentState.upgrading
        )
        {
            return;
        }


        /*
         * Take the gold.
         */

        bool started =
    GameManager.Instance
        .StartBuildingUpgrade(
            buildingName,
            upgradeCost,
            upgradeTime
        );

        if (!started)
        {
            if (
                UIManager.Instance != null
            )
            {
                UIManager.Instance
                    .UpdateMissionStatus(
                        "NOT ENOUGH GOLD"
                    );
            }

            return;
        }


        if (!started)
        {
            /*
             * StartBuildingUpgrade normally handles
             * the gold itself. Because we already spent
             * it above, return it if something failed.
             */

            GameManager.Instance
                .AddGold(upgradeCost);

            return;
        }


        /*
 * Make this the active construction.
 *
 * IMPORTANT:
 * We do NOT automatically assign builders here.
 *
 * The + Builder button controls how many
 * crew members are assigned.
 */

        CrewManager.Instance
            .activeConstruction = this;

        Debug.Log(
            "CONSTRUCTION READY | " +
            buildingName +
            " | Builders: 0"
        );


        /*
         * Construction visual.
         */

        RefreshConstructionVisuals();

        ShowWorldProgress();


        if (
            UIManager.Instance != null
        )
        {
            UIManager.Instance
                .UpdateMissionStatus(
                    buildingName +
                    " UPGRADING..."
                );
        }


        Debug.Log(
            "BUILDING UPGRADE STARTED | " +
            buildingName
        );
    }


    // =========================================================
    // CONSTRUCTION STARTED
    // =========================================================

    public void ConstructionStarted()
    {
        if (
            GameManager.Instance == null
        )
        {
            return;
        }


        GameManager.Instance
            .ConstructionStarted(
                buildingName
            );


        Debug.Log(
            "BUILDERS REACHED BUILDING | " +
            buildingName
        );
    }


    // =========================================================
    // REFRESH CONSTRUCTION VISUAL
    // =========================================================

    private void RefreshConstructionVisuals()
    {
        if (
            persistentState == null
        )
        {
            return;
        }


        if (
            constructionVisual != null
        )
        {
            constructionVisual.SetActive(
                persistentState.upgrading
            );
        }


        /*
         * If the building is no longer upgrading,
         * make sure the finished building model
         * is displayed.
         */

        if (
            !persistentState.upgrading
        )
        {
            RefreshVisuals();
        }
    }


    // =========================================================
    // WORLD PROGRESS UI
    // =========================================================

    private void ShowWorldProgress()
    {
        if (
            worldUpgradeSlider != null
        )
        {
            worldUpgradeSlider.gameObject
                .SetActive(true);
        }


        if (
            worldProgressText != null
        )
        {
            worldProgressText.gameObject
                .SetActive(true);
        }


        if (
            worldSpeedText != null
        )
        {
            worldSpeedText.gameObject
                .SetActive(true);
        }
    }


    private void HideWorldProgress()
    {
        if (
            worldUpgradeSlider != null
        )
        {
            worldUpgradeSlider.gameObject
                .SetActive(false);
        }


        if (
            worldProgressText != null
        )
        {
            worldProgressText.gameObject
                .SetActive(false);
        }


        if (
            worldSpeedText != null
        )
        {
            worldSpeedText.gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // FINISH VISUALS
    // =========================================================

    public void RefreshVisuals()
    {
        if (
            levelModels == null ||
            levelModels.Length == 0
        )
        {
            return;
        }


        for (
            int i = 0;
            i < levelModels.Length;
            i++
        )
        {
            if (
                levelModels[i] != null
            )
            {
                levelModels[i]
                    .SetActive(false);
            }
        }


        int levelIndex =
            Mathf.Clamp(
                buildingLevel - 1,
                0,
                levelModels.Length - 1
            );


        if (
            levelModels[levelIndex] != null
        )
        {
            levelModels[levelIndex]
                .SetActive(true);
        }
    }


    // =========================================================
    // GET PROGRESS
    // =========================================================

    public float GetUpgradePercent()
    {
        if (
            persistentState == null
        )
        {
            return 0f;
        }


        return
            persistentState.GetProgress();
    }


    public float GetUpgradeProgress()
    {
        if (
            persistentState == null
        )
        {
            return 0f;
        }


        return
            persistentState.GetProgress();
    }


    // =========================================================
    // CURRENT LEVEL DATA
    // =========================================================

    public BuildingLevelData CurrentLevelData()
    {
        if (
            levels == null ||
            levels.Length == 0
        )
        {
            return null;
        }


        int index =
            Mathf.Clamp(
                buildingLevel - 1,
                0,
                levels.Length - 1
            );


        return levels[index];
    }
}