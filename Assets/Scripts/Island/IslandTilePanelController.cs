
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IslandTilePanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IslandTileSelection tileSelection;
    [SerializeField] private GameObject tilePanel;

    [Header("Tile Information")]
    [SerializeField] private TMP_Text tileNameText;
    [SerializeField] private TMP_Text coordinatesText;
    [SerializeField] private TMP_Text terrainText;
    [SerializeField] private TMP_Text ownershipText;
    [SerializeField] private TMP_Text developmentText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button developButton;

    private void Awake()
    {
        if (tileSelection == null)
            tileSelection = GetComponent<IslandTileSelection>();

        if (tilePanel != null)
            tilePanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (tileSelection != null)
            tileSelection.SelectionChanged += OnTileSelected;

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (developButton != null)
            developButton.onClick.AddListener(OnDevelopClicked);
    }

    private void OnDisable()
    {
        if (tileSelection != null)
            tileSelection.SelectionChanged -= OnTileSelected;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (developButton != null)
            developButton.onClick.RemoveListener(OnDevelopClicked);
    }

    private void OnTileSelected(IslandTile tile)
    {
        if (tile == null)
        {
            tilePanel.SetActive(false);
            return;
        }

        tileNameText.text = tile.terrainType.ToString().ToUpper();
        coordinatesText.text = $"Coordinates: ({tile.q}, {tile.r})";
        terrainText.text = $"Terrain: {tile.terrainType}";
        ownershipText.text = tile.owned ? "Ownership: Owned" : "Ownership: Unowned";
        developmentText.text = $"Development: {tile.tileState}";

        // Development functionality will be added separately.
        developButton.interactable = false;

        tilePanel.SetActive(true);
    }

    private void ClosePanel()
    {
        tileSelection.ClearSelection();
    }

    private void OnDevelopClicked()
    {
        Debug.Log("ISLAND TILE | Development system not implemented yet.");
    }
}
