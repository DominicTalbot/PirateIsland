
using System.Collections.Generic;
using UnityEngine;

public class IslandObstacleVisuals : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject treePrefab;

    private IslandTileVisuals tileVisuals;

    private readonly Dictionary<IslandObstacle, IslandObstacleVisual>
        spawnedTrees = new Dictionary<IslandObstacle, IslandObstacleVisual>();

    private readonly Dictionary<IslandTile, Transform> obstacleRoots =
    new Dictionary<IslandTile, Transform>();

    private void Awake()
    {
        tileVisuals = GetComponent<IslandTileVisuals>();

        if (tileVisuals == null)
        {
            Debug.LogError(
                "OBSTACLE VISUALS | IslandTileVisuals component missing."
            );
        }

        if (treePrefab == null)
        {
            Debug.LogError(
                "OBSTACLE VISUALS | Tree prefab has not been assigned."
            );
        }
    }


    private void AlignTreeToTile(GameObject tree, GameObject tileVisual)
    {
        Renderer[] renderers = tree.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds treeBounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            treeBounds.Encapsulate(renderer.bounds);
        }

        Collider tileCollider = tileVisual.GetComponentInChildren<MeshCollider>();

        if (tileCollider == null)
        {
            Debug.LogWarning("TREE PLACEMENT | Tile MeshCollider missing.");
            return;
        }

        float tileSurface = tileCollider.bounds.max.y;
        float treeBottom = treeBounds.min.y;

        tree.transform.position += Vector3.up * (tileSurface - treeBottom);
    }


    public void SpawnObstaclesForTile(IslandTile tile)
    {
        if (tile == null)
            return;

        if (tileVisuals == null || treePrefab == null)
            return;

        GameObject tileVisual = tileVisuals.GetTileVisual(tile);

        if (tileVisual == null)
        {
            Debug.LogError(
                "OBSTACLE VISUALS | Could not find visual for tile (" +
                tile.q + ", " + tile.r + ")."
            );
            return;
        }

        Transform obstacleRoot;

        if (!obstacleRoots.TryGetValue(tile, out obstacleRoot))
        {
            GameObject root = new GameObject("ObstacleVisualRoot");

            root.transform.SetParent(tileVisual.transform, false);

            // Keep the obstacle container at the tile's centre,
            // but cancel out the tile's rotation.
            root.transform.position = tileVisual.transform.position;
            root.transform.rotation = Quaternion.identity;

            obstacleRoot = root.transform;
            obstacleRoots.Add(tile, obstacleRoot);
        }

        int treesSpawned = 0;

        foreach (IslandObstacle obstacle in tile.obstacles)
        {
            if (obstacle == null)
                continue;

            // This first version only displays trees.
            if (obstacle.obstacleType != IslandObstacleType.Tree)
                continue;

            // Prevent the same obstacle from being spawned twice.
            if (spawnedTrees.ContainsKey(obstacle))
                continue;

            GameObject tree = Instantiate(
                treePrefab,
                obstacleRoot,
                false
            );

            tree.name =
                "Tree_" + tile.q + "_" + tile.r + "_" + treesSpawned;

            // Keep the tree upright, regardless of prefab rotation.
            tree.transform.rotation = Quaternion.identity;

            // Position it at its generated location on the tile.
            // Apply the tree model's orientation correction.
            tree.transform.localRotation = Quaternion.Euler(-89.98f, 0f, 0f);

            // Apply its generated position on the tile.
            tree.transform.localPosition = obstacle.localPosition;

            // Align the trunk with the tile surface.
            AlignTreeToTile(tree, tileVisual);
            

            // Find the bottom of the visible tree.
            Renderer[] renderers = tree.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds treeBounds = renderers[0].bounds;

                foreach (Renderer renderer in renderers)
                {
                    treeBounds.Encapsulate(renderer.bounds);
                }

                // Find the top surface of the tile.
                Collider tileCollider = tileVisual.GetComponent<Collider>();

                if (tileCollider != null)
                {
                    float tileSurfaceY = tileCollider.bounds.max.y;
                    float treeBottomY = treeBounds.min.y;

                    // Move the tree so its trunk base touches the tile.
                    tree.transform.position += Vector3.up *
                        (tileSurfaceY - treeBottomY);
                }
            }

            IslandObstacleVisual obstacleVisual =
                tree.GetComponent<IslandObstacleVisual>();

            if (obstacleVisual == null)
            {
                obstacleVisual =
                    tree.AddComponent<IslandObstacleVisual>();
            }

            obstacleVisual.Initialize(tile, obstacle);

            spawnedTrees.Add(obstacle, obstacleVisual);

            treesSpawned++;
        }

        Debug.Log(
            "OBSTACLE VISUALS | Trees spawned: " + treesSpawned +
            " | Tile: (" + tile.q + ", " + tile.r + ")"
        );
    }

    public IslandObstacleVisual GetObstacleVisual(IslandObstacle obstacle)
    {
        if (obstacle == null)
            return null;

        spawnedTrees.TryGetValue(
            obstacle,
            out IslandObstacleVisual visual
        );

        return visual;
    }

    public void RemoveObstacleVisual(IslandObstacle obstacle)
    {
        if (obstacle == null)
            return;

        if (!spawnedTrees.TryGetValue(
            obstacle,
            out IslandObstacleVisual visual))
        {
            return;
        }

        spawnedTrees.Remove(obstacle);

        if (visual != null)
            Destroy(visual.gameObject);
    }

    public int GetSpawnedTreeCount()
    {
        return spawnedTrees.Count;
    }
}
