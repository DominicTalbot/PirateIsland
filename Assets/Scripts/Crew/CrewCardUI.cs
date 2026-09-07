using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrewCardUI : MonoBehaviour
{
    public Image portrait;
    public TextMeshProUGUI crewName;
    public TextMeshProUGUI crewJob;
    public TextMeshProUGUI crewStatus;

    private Button button;
    private Image cardBackground;
    private bool isSelected;

    private Action<CrewCardUI> selectionCallback;

    public CrewMovement Crew { get; private set; }

    public bool IsSelected
    {
        get { return isSelected; }
    }

    private readonly Color normalColor = new Color(0.68f, 0.68f, 0.68f);
    private readonly Color selectedColor = new Color(0.82f, 0.65f, 0.25f);

    public void Setup(
        CrewMovement crew,
        Sprite crewPortrait,
        Action<CrewCardUI> onSelectionChanged
    )
    {
        if (crew == null || crew.crewData == null)
        {
            return;
        }

        Crew = crew;
        selectionCallback = onSelectionChanged;

        crewName.text = crew.crewData.crewName;

        crewJob.text =
            crew.currentJob.ToString().ToUpper();

        if (portrait != null)
        {
            portrait.sprite = crewPortrait;
        }

        button = GetComponent<Button>();
        cardBackground = GetComponent<Image>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ToggleSelected);
        }

        isSelected = false;

        UpdateVisual();
    }

    private void ToggleSelected()
    {
        if (selectionCallback != null)
        {
            selectionCallback(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (crewStatus != null)
        {
            crewStatus.text =
                isSelected
                    ? "SELECTED"
                    : "AVAILABLE";
        }

        if (cardBackground != null)
        {
            cardBackground.color =
                isSelected
                    ? selectedColor
                    : normalColor;
        }
    }
}