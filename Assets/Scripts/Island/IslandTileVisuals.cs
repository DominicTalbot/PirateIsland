
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

        if (tilePrefab == null)
        {
            Debug.LogError(
                "ISLAND TILE VISUALS | Assign the GreenHexTile prefab."
            );
            return;
        }

        if (visualsRoot == null)
        {
            Debug.LogError(
                "ISLAND TILE VISUALS | Assign IslandTileVisualsRoot."
            );
            return;
        }

        GenerateOwnedTiles();
    }

    private void GenerateOwnedTiles()
    {
        foreach (IslandTile tile in dataManager.Tiles)
        {
            if (!tile.owned)
            {
                continue;
            }

            float tileSize = islandGrid.tileSize;

            float x =
                tileSize *
                Mathf.Sqrt(3f) *
                (tile.q + tile.r * 0.5f);

            float z =
                tileSize *
                1.5f *
                tile.r;

            Vector3 position =
    visualsRoot.position +
    new Vector3(x, 0f, z);

            position.y = tileHeight;

            GameObject visualTile = Instantiate(
    tilePrefab,
    position,
    tilePrefab.transform.rotation,
    visualsRoot
);

            visualTile.name =
                $"IslandTile_{tile.q}_{tile.r}";

            spawnedTiles.Add(visualTile);
        }

        Debug.Log(
            "ISLAND TILE VISUALS CREATED | Tiles: " +
            spawnedTiles.Count
        );
    }
}
