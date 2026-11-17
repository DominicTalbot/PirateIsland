
using System.Collections.Generic;
using UnityEngine;

public class IslandTileVisuals : MonoBehaviour
{
    [Header("Tile Prefab")]
    public GameObject tilePrefab;

    [Header("Placement")]
    public float tileHeight = 0.055f;
    public Transform visualsRoot;

    private IslandDataManager dataManager;
    private IslandGrid islandGrid;

    private readonly Dictionary<IslandTile, GameObject> tileVisualLookup =
        new Dictionary<IslandTile, GameObject>();

    private readonly Dictionary<IslandTile, IslandTileSelectable>
        tileSelectableLookup =
        new Dictionary<IslandTile, IslandTileSelectable>();

    private readonly Dictionary<IslandTile, Transform>
        developmentVisualRoots =
        new Dictionary<IslandTile, Transform>();

    private readonly Dictionary<IslandTile, GameObject>
        tileMeshLookup =
        new Dictionary<IslandTile, GameObject>();

    private readonly List<GameObject> spawnedTiles =
        new List<GameObject>();

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

            // Create an upright positioning root for the tile.
            GameObject tileRoot = new GameObject(
                $"IslandTile_{tile.q}_{tile.r}"
            );

            tileRoot.transform.SetParent(visualsRoot, true);
            tileRoot.transform.position = position;
            tileRoot.transform.rotation = Quaternion.identity;
            tileRoot.transform.localScale = Vector3.one;

            // Put the original tile prefab underneath the neutral root.
            GameObject tileMesh = Instantiate(
                tilePrefab,
                tileRoot.transform,
                false
            );

            tileMesh.name = "TileMesh";
            tileMesh.transform.localPosition = Vector3.zero;
            tileMesh.transform.localRotation =
                tilePrefab.transform.localRotation;
            tileMesh.transform.localScale =
                tilePrefab.transform.localScale;

            // Selection lives on the neutral root.
            IslandTileSelectable selectable =
                tileRoot.AddComponent<IslandTileSelectable>();

            selectable.Initialize(tile);

            spawnedTiles.Add(tileRoot);

            tileVisualLookup.Add(tile, tileRoot);
            tileSelectableLookup.Add(tile, selectable);
            tileMeshLookup.Add(tile, tileMesh);
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

        if (tileVisualLookup.TryGetValue(
            tile, out GameObject visual))
        {
            return visual;
        }

        return null;
    }

    public bool ReplaceDevelopmentVisual(
        IslandTile tile,
        GameObject developmentPrefab)
    {
        if (tile == null || developmentPrefab == null)
            return false;

        GameObject tileRoot = GetTileVisual(tile);

        if (tileRoot == null)
        {
            Debug.LogWarning(
                "DEVELOPMENT VISUAL | Tile root not found."
            );
            return false;
        }

        if (!developmentVisualRoots.TryGetValue(
    tile, out Transform visualRoot))
        {
            GameObject root = new GameObject(
                "DevelopmentVisualRoot"
            );

            root.transform.SetParent(tileRoot.transform, false);

            visualRoot = root.transform;
            developmentVisualRoots.Add(tile, visualRoot);
        }

        visualRoot.localRotation = Quaternion.identity;

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localScale = Vector3.one;

        // Remove the previous development visual.
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(visualRoot.GetChild(i).gameObject);
        }

        // Add the complete development prefab.
        GameObject developmentVisual = Instantiate(
            developmentPrefab,
            visualRoot,
            false
        );

        developmentVisual.transform.localPosition = Vector3.zero;
        developmentVisual.transform.localRotation = Quaternion.identity;
        developmentVisual.transform.localScale = Vector3.one;

        // Hide only the original tile mesh.
        if (tileMeshLookup.TryGetValue(
            tile, out GameObject tileMesh))
        {
            Renderer[] renderers =
                tileMesh.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }

        // Refresh selection appearance.
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
