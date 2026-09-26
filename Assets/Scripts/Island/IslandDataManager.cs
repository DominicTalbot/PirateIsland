using System.Collections.Generic;
using UnityEngine;

public class IslandDataManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int radius = 4;

    public int startingRadius = 2;

    private readonly List<IslandTile> tiles =
        new List<IslandTile>();

    public IReadOnlyList<IslandTile> Tiles => tiles;

    private void Awake()
    {
        GenerateInitialGrid();
    }

    private void GenerateInitialGrid()
    {
        tiles.Clear();

        for (int q = -radius; q <= radius; q++)
        {
            int rMin = Mathf.Max(-radius, -q - radius);
            int rMax = Mathf.Min(radius, -q + radius);

            for (int r = rMin; r <= rMax; r++)
            {
                IslandTile tile =
    new IslandTile(
        q,
        r,
        IslandTerrainType.Grass
    );

                int distance =
                    Mathf.Max(
                        Mathf.Abs(q),
                        Mathf.Abs(r),
                        Mathf.Abs(-q - r)
                    );

                if (IsStartingTile(q, r))
                {
                    tile.owned = true;
                    tile.tileState = IslandTileState.Available;
                }

                tiles.Add(tile);
            }
        }

        UpdateCoastalEdges();

        AssignCoastalTerrain();

        Debug.Log(
            "ISLAND DATA CREATED | Tiles: " +
            tiles.Count
        );

        int ownedCount = 0;

        foreach (IslandTile tile in tiles)
        {
            if (tile.owned)
            {
                ownedCount++;
            }
        }

        Debug.Log(
            "STARTING ISLAND | Owned Tiles: " +
            ownedCount
        );

        int coastalEdgeCount = 0;

        foreach (IslandTile tile in tiles)
        {
            if (!tile.owned)
            {
                continue;
            }

            foreach (IslandTileEdge edge in tile.edges)
            {
                if (edge.edgeType == IslandEdgeType.Ocean)
                {
                    coastalEdgeCount++;
                }
            }
        }

        Debug.Log(
            "STARTING ISLAND | Coastal Edges: " +
            coastalEdgeCount
        );
    }

    private bool IsStartingTile(int q, int r)
    {
        int distance =
            Mathf.Max(
                Mathf.Abs(q),
                Mathf.Abs(r),
                Mathf.Abs(-q - r)
            );

        return distance <= startingRadius;
    }

    public IslandTile GetTile(int q, int r)
    {
        foreach (IslandTile tile in tiles)
        {
            if (tile.q == q && tile.r == r)
            {
                return tile;
            }
        }

        return null;
    }

    private void UpdateCoastalEdges()
    {
        foreach (IslandTile tile in tiles)
        {
            if (!tile.owned)
            {
                continue;
            }

            for (int edge = 0; edge < 6; edge++)
            {
                int neighbourQ = tile.q;
                int neighbourR = tile.r;

                switch (edge)
                {
                    case 0:
                        neighbourQ++;
                        break;

                    case 1:
                        neighbourR++;
                        break;

                    case 2:
                        neighbourQ--;
                        neighbourR++;
                        break;

                    case 3:
                        neighbourQ--;
                        break;

                    case 4:
                        neighbourR--;
                        break;

                    case 5:
                        neighbourQ++;
                        neighbourR--;
                        break;
                }

                IslandTile neighbour =
                    GetTile(neighbourQ, neighbourR);

                if (neighbour == null || !neighbour.owned)
                {
                    tile.edges[edge].edgeType =
                        IslandEdgeType.Ocean;
                }
                else
                {
                    tile.edges[edge].edgeType =
                        IslandEdgeType.Land;
                }
            }
        }
    }

    private void AssignCoastalTerrain()
    {
        foreach (IslandTile tile in tiles)
        {
            if (!tile.owned)
            {
                continue;
            }

            bool isCoastal = false;

            foreach (IslandTileEdge edge in tile.edges)
            {
                if (edge.edgeType == IslandEdgeType.Ocean)
                {
                    isCoastal = true;
                    break;
                }
            }

            if (isCoastal)
            {
                tile.terrainType =
                    IslandTerrainType.Beach;
            }
            else
            {
                tile.terrainType =
                    IslandTerrainType.Grass;
            }
        }

        int beachCount = 0;

        foreach (IslandTile tile in tiles)
        {
            if (tile.terrainType == IslandTerrainType.Beach)
            {
                beachCount++;
            }
        }

        Debug.Log(
            "STARTING ISLAND | Beach Tiles: " +
            beachCount
        );
    }
}