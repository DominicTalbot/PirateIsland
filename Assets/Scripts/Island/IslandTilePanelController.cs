
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IslandTilePanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IslandTileSelection tileSelection;
    [SerializeField] private IslandDataManager islandDataManager;
    [SerializeField] private IslandDevelopmentManager developmentManager;
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

    private TMP_Text developButtonText;
    private IslandTile selectedTile;

    private void Awake()
    {
        if (tileSelection == null)
            tileSelection = GetComponent<IslandTileSelection>();

        if (islandDataManager == null)
            islandDataManager = FindFirstObjectByType<IslandDataManager>();

        if (developButton != null)
            developButtonText = developButton.GetComponentInChildren<TMP_Text>();

        if (developmentManager == null)
            developmentManager = FindFirstObjectByType<IslandDevelopmentManager>();

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
        selectedTile = tile;

        if (tile == null)
        {
            tilePanel.SetActive(false);
            return;
        }

        tileNameText.text = tile.terrainType.ToString().ToUpper();
        coordinatesText.text = $"Coordinates: ({tile.q}, {tile.r})";
        terrainText.text = $"Terrain: {tile.terrainType}";
        ownershipText.text = tile.owned
            ? "Ownership: Owned"
            : "Ownership: Unowned";

        developmentText.text = $"Development: {tile.tileState}";

        if (tile.owned)
        {
            bool canDevelop =
                developmentManager != null &&
                developmentManager.CanDevelopTile(tile) &&
                developmentManager.GetDevelopmentDefinition(
                    IslandDevelopmentType.Farm
                ) != null;

            developButton.interactable = canDevelop;

            if (developButtonText != null)
            {
                if (canDevelop)
                    developButtonText.text = "Build Farm";
                else if (!tile.isExpansionTile)
                    developButtonText.text = "Starting Island";
                else if (tile.tileState == IslandTileState.Clearing)
                    developButtonText.text = "Still Clearing";
                else if (tile.developmentType != IslandDevelopmentType.None)
                    developButtonText.text = "Already Developed";
                else
                    developButtonText.text = "Unavailable";
            }
        }
        else
        {
            int cost = islandDataManager.GetTilePurchaseCost(tile);

            bool connected = islandDataManager.CanPurchaseTile(tile);

            bool canAfford = GameManager.Instance != null &&
                             GameManager.Instance.gold >= cost;

            developButton.interactable = connected && canAfford;

            if (developButtonText != null)
            {
                if (!connected)
                    developButtonText.text = "Requires Adjacent Land";
                else if (!canAfford)
                    developButtonText.text = "Not Enough Gold";
                else
                    developButtonText.text = $"Purchase Land - {cost} Gold";
            }
        }

        tilePanel.SetActive(true);
    }

    private void ClosePanel()
    {
        tileSelection.ClearSelection();
    }

    private void OnDevelopClicked()
    {
        if (selectedTile == null)
            return;

        if (selectedTile.owned)
        {
            if (developmentManager == null)
                return;

            bool developed = developmentManager.TryDevelopTile(
                selectedTile,
                IslandDevelopmentType.Farm
            );

            if (developed)
            {
                tileSelection.RefreshSelectedTileAppearance();
                OnTileSelected(selectedTile);
            }

            return;
        }

        if (islandDataManager == null)
        {
            Debug.LogError("ISLAND TILE | IslandDataManager missing.");
            return;
        }

        bool purchased = islandDataManager.TryPurchaseTile(selectedTile);

        if (purchased)
        {
            tileSelection.RefreshSelectedTileAppearance();
            OnTileSelected(selectedTile);
        }
    }
}
