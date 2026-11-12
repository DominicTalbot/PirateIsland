
using System.Collections.Generic;
using UnityEngine;

public class IslandTileVisuals : MonoBehaviour
{
    [Header("Tile Prefab")]
    public GameObject tilePrefab;

    [Header("Placement")]
    public float tileHeight = 0.055f;
    public Transform visualsRoot;

    private readonly Dictionary<IslandTile, Transform>
    developmentVisualRoots = new Dictionary<IslandTile, Transform>();

    private IslandDataManager dataManager;
    private IslandGrid islandGrid;
    private readonly Dictionary<IslandTile, IslandTileSelectable>
    tileSelectableLookup = new Dictionary<IslandTile, IslandTileSelectable>();

    private readonly List<GameObject> spawnedTiles =
        new List<GameObject>();

    private readonly Dictionary<IslandTile, GameObject> tileVisualLookup =
    new Dictionary<IslandTile, GameObject>();

    private void Start()
    {
        dataManager = GetComponent<IslandDataManager>();
        islandGrid = GetComponent<IslandGrid>();

        if (dataManager == null || islandGrid == null)
        {
            Debug.LogError(
                "ISLAND TILE VISUALS | IslandDataManager or IslandGrid missing."
            );
            return;
        }

        if (tilePrefab == null || visualsRoot == null)
        {
            Debug.LogError(
                "ISLAND TILE VISUALS | Tile prefab or visuals root missing."
            );
            return;
        }

        GenerateTiles();
    }

    private void GenerateTiles()
    {
        foreach (IslandTile tile in dataManager.Tiles)
        {
            float tileSize = islandGrid.tileSize;

            float x =
                tileSize * Mathf.Sqrt(3f) *
                (tile.q + tile.r * 0.5f);

            float z =
                tileSize * 1.5f * tile.r;

            Vector3 position =
                visualsRoot.position + new Vector3(x, 0f, z);

            position.y = tileHeight;

            GameObject visualTile = Instantiate(
                tilePrefab,
                position,
                tilePrefab.transform.rotation,
                visualsRoot
            );

            visualTile.name =
                $"IslandTile_{tile.q}_{tile.r}";

            IslandTileSelectable selectable =
                visualTile.GetComponent<IslandTileSelectable>();

            if (selectable == null)
            {
                selectable =
                    visualTile.AddComponent<IslandTileSelectable>();
            }

            selectable.Initialize(tile);

            spawnedTiles.Add(visualTile);

            tileVisualLookup.Add(tile, visualTile);

            tileSelectableLookup.Add(tile, selectable);
        }

        Debug.Log(
            "ISLAND TILE VISUALS CREATED | Tiles: " +
            spawnedTiles.Count
        );
    }

    public GameObject GetTileVisual(IslandTile tile)
    {
        if (tile == null)
            return null;

        if (tileVisualLookup.TryGetValue(tile, out GameObject visual))
            return visual;

        return null;
    }

    public bool ReplaceDevelopmentVisual(
        IslandTile tile,
        GameObject developmentPrefab)
    {
        if (tile == null || developmentPrefab == null)
            return false;

        GameObject tileVisual = GetTileVisual(tile);

        if (tileVisual == null)
        {
            Debug.LogWarning("DEVELOPMENT VISUAL | Tile visual not found.");
            return false;
        }

        if (!developmentVisualRoots.TryGetValue(
            tile, out Transform visualRoot))
        {
            GameObject root = new GameObject("DevelopmentVisualRoot");
            root.transform.SetParent(tileVisual.transform, false);

            visualRoot = root.transform;
            developmentVisualRoots.Add(tile, visualRoot);
        }

        // Remove the previous development visual, if one exists.
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(visualRoot.GetChild(i).gameObject);
        }

        // Add the complete development prefab.
        GameObject developmentVisual = Instantiate(
            developmentPrefab,
            visualRoot
        );

        developmentVisual.transform.localPosition = Vector3.zero;
        developmentVisual.transform.localRotation = Quaternion.identity;
        developmentVisual.transform.localScale = Vector3.one;

        // Hide the original tile mesh, but keep its collider.
        Renderer originalRenderer = tileVisual.GetComponent<Renderer>();

        if (originalRenderer != null)
            originalRenderer.enabled = false;

        // Refresh selection so the new child renderers are recognised.
        if (tileSelectableLookup.TryGetValue(
            tile, out IslandTileSelectable selectable))
        {
            selectable.RefreshAppearance();
        }

        Debug.Log(
            "DEVELOPMENT VISUAL CREATED | " +
            tile.developmentName +
            " | Tile: (" + tile.q + ", " + tile.r + ")"
        );

        return true;
    }

}
