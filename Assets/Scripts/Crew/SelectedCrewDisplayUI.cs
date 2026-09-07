using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedCrewDisplayUI : MonoBehaviour
{
    public Image portrait;
    public TextMeshProUGUI crewName;
    public TextMeshProUGUI crewJob;

    public void Setup(CrewMovement crew, Sprite crewPortrait)
    {
        if (crew == null || crew.crewData == null)
        {
            return;
        }

        if (portrait != null)
        {
            portrait.sprite = crewPortrait;
        }

        if (crewName != null)
        {
            crewName.text = crew.crewData.crewName;
        }

        if (crewJob != null)
        {
            crewJob.text = crew.currentJob.ToString().ToUpper();
        }
    }
}