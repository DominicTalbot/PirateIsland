using TMPro;
using UnityEngine;

public class VoyageUI : MonoBehaviour
{
    public TextMeshProUGUI voyageText;

    private void Update()
    {
        if (
            VoyageManager.Instance == null
        )
        {
            return;
        }

        string output = "";

        foreach (
            VoyageData voyage
            in VoyageManager.Instance
            .activeVoyages
        )
        {
            output +=
    "image of anchor " +
    voyage.voyageName +
    " (" +
    Mathf.RoundToInt(
        voyage.progress
    ) +
    "%)";

            if (
                voyage.needsAttention
            )
            {
                output +=
                    "NEEDS ATTENTION";
            }

            output += "\n";
        }

        voyageText.text = output;
    }
}