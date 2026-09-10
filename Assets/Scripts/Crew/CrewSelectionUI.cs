using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrewSelectionUI : MonoBehaviour
{
    public Transform content;
    public CrewCardUI crewCardTemplate;

    [Header("Crew Portraits")]
    public Sprite crew1Portrait;
    public Sprite crew2Portrait;
    public Sprite crew3Portrait;
    public Sprite crew4Portrait;
    public Sprite crew5Portrait;

    [Header("Selection UI")]
    public TextMeshProUGUI selectionCountText;
    public Button confirmCrewButton;

    [Header("Selection Settings")]
    public int maxCrewSelection = 5;

    private readonly List<CrewMovement> selectedCrew =
        new List<CrewMovement>();

    private void OnEnable()
    {
        PopulateCrew();
        UpdateSelectionUI();
    }

    private void PopulateCrew()
    {
        if (CrewManager.Instance == null)
        {
            Debug.LogWarning(
                "CrewSelectionUI: CrewManager is missing."
            );

            return;
        }

        if (content == null)
        {
            Debug.LogWarning(
                "CrewSelectionUI: Content is missing."
            );

            return;
        }

        if (crewCardTemplate == null)
        {
            Debug.LogWarning(
                "CrewSelectionUI: Crew Card Template is missing."
            );

            return;
        }

        crewCardTemplate.gameObject.SetActive(false);

        selectedCrew.Clear();

        foreach (Transform child in content)
        {
            if (child != crewCardTemplate.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (CrewMovement crew in CrewManager.Instance.crewMembers)
        {
            if (crew == null || crew.crewData == null)
            {
                continue;
            }

            CrewData data = crew.crewData;

            // Don't show crew already away on a voyage.
            if (data.isOnVoyage)
            {
                continue;
            }

            // Don't show crew currently working on the island.
            if (
                data.islandJob == CrewIslandJob.Fishing ||
                data.islandJob == CrewIslandJob.Building
            )
            {
                continue;
            }

            CrewCardUI newCard =
                Instantiate(
                    crewCardTemplate,
                    content
                );

            newCard.gameObject.SetActive(true);

            Sprite portrait =
                GetCrewPortrait(data.crewId);

            newCard.Setup(
                crew,
                portrait,
                HandleCrewCardSelected
            );
        }
    }

    public Sprite GetCrewPortrait(string crewId)
    {
        switch (crewId)
        {
            case "crew_001":
                return crew1Portrait;

            case "crew_002":
                return crew2Portrait;

            case "crew_003":
                return crew3Portrait;

            case "crew_004":
                return crew4Portrait;

            case "crew_005":
                return crew5Portrait;

            default:
                return null;
        }
    }

    private void HandleCrewCardSelected(CrewCardUI card)
    {
        if (card == null || card.Crew == null)
        {
            return;
        }

        if (selectedCrew.Contains(card.Crew))
        {
            selectedCrew.Remove(card.Crew);

            card.SetSelected(false);

            UpdateSelectionUI();

            return;
        }

        if (selectedCrew.Count >= maxCrewSelection)
        {
            Debug.Log(
                "CREW SELECTION FULL | Maximum crew: " +
                maxCrewSelection
            );

            return;
        }

        selectedCrew.Add(card.Crew);

        card.SetSelected(true);

        UpdateSelectionUI();

        Debug.Log(
            "CREW SELECTED: " +
            card.Crew.crewData.crewName
        );
    }

    private void UpdateSelectionUI()
    {
        if (selectionCountText != null)
        {
            selectionCountText.text =
                selectedCrew.Count +
                " / " +
                maxCrewSelection +
                " CREW SELECTED";
        }

        if (confirmCrewButton != null)
        {
            confirmCrewButton.interactable =
                selectedCrew.Count > 0;
        }
    }

    public List<CrewMovement> GetSelectedCrew()
    {
        return new List<CrewMovement>(selectedCrew);
    }

    public void BackToMissions()
    {
        selectedCrew.Clear();

        foreach (CrewCardUI card in content.GetComponentsInChildren<CrewCardUI>())
        {
            if (card != crewCardTemplate)
            {
                card.SetSelected(false);
            }
        }

        gameObject.SetActive(false);

        if (UIManager.Instance != null && UIManager.Instance.missionsPanel != null)
        {
            UIManager.Instance.missionsPanel.SetActive(true);
        }
    }

    public void ConfirmCrewSelection()
    {
        if (selectedCrew.Count == 0)
        {
            Debug.Log("NO CREW SELECTED");
            return;
        }

        Debug.Log(
            "CREW SELECTION CONFIRMED | Crew Count: "
            + selectedCrew.Count
        );

        foreach (CrewMovement crew in selectedCrew)
        {
            Debug.Log(
                "SELECTED CREW: "
                + crew.crewData.crewId
                + " | "
                + crew.crewData.crewName
            );
        }

        if (UIManager.Instance != null)
        {
            MissionManager.Instance.SetSelectedMissionCrew(
    selectedCrew
);

            UIManager.Instance.DisplaySelectedCrew(
                selectedCrew
            );

            gameObject.SetActive(false);

            if (UIManager.Instance.missionsPanel != null)
            {
                UIManager.Instance.missionsPanel.SetActive(true);
            }
        }
    }
}