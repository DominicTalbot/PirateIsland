
using System.Collections.Generic;
using UnityEngine;

public class IslandNavigation : MonoBehaviour
{
    private IslandDataManager dataManager;
    private IslandTileVisuals tileVisuals;
    private readonly HashSet<IslandTile> obstructedTiles =
    new HashSet<IslandTile>();

    private static readonly int[,] Directions =
    {
        { 0, 1 },
        { -1, 1 },
        { -1, 0 },
        { 0, -1 },
        { 1, -1 },
        { 1, 0 }
    };

    private void Awake()
    {
        dataManager = GetComponent<IslandDataManager>();
        tileVisuals = GetComponent<IslandTileVisuals>();
    }

    public bool TryFindPath(
        IslandTile startTile,
        IslandTile destinationTile,
        out List<Vector3> path)
    {
        path = new List<Vector3>();

        if (dataManager == null || tileVisuals == null)
        {
            Debug.LogError("ISLAND NAVIGATION | Missing island components.");
            return false;
        }

        if (startTile == null || destinationTile == null)
            return false;

        if (!startTile.owned || !IsWalkable(destinationTile))
            return false;

        if (startTile == destinationTile)
            return true;

        List<IslandTile> openTiles = new List<IslandTile>();

        Dictionary<IslandTile, IslandTile> cameFrom =
            new Dictionary<IslandTile, IslandTile>();

        Dictionary<IslandTile, int> gScore =
            new Dictionary<IslandTile, int>();

        Dictionary<IslandTile, int> fScore =
            new Dictionary<IslandTile, int>();

        openTiles.Add(startTile);
        gScore[startTile] = 0;
        fScore[startTile] = GetDistance(startTile, destinationTile);

        while (openTiles.Count > 0)
        {
            IslandTile current = GetLowestScoreTile(openTiles, fScore);

            if (current == destinationTile)
            {
                BuildPath(cameFrom, current, path);
                return true;
            }

            openTiles.Remove(current);

            for (int i = 0; i < 6; i++)
            {
                int q = current.q + Directions[i, 0];
                int r = current.r + Directions[i, 1];

                IslandTile neighbour = dataManager.GetTile(q, r);

                if (neighbour == null || !IsWalkable(neighbour))
                    continue;

                int tentativeScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbour) ||
                    tentativeScore < gScore[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeScore;
                    fScore[neighbour] =
                        tentativeScore + GetDistance(neighbour, destinationTile);

                    if (!openTiles.Contains(neighbour))
                        openTiles.Add(neighbour);
                }
            }
        }

        return false;
    }

    public void SetTileObstructed(IslandTile tile, bool obstructed)
    {
        if (tile == null)
            return;

        if (obstructed)
        {
            obstructedTiles.Add(tile);
        }
        else
        {
            obstructedTiles.Remove(tile);
        }
    }

    public bool IsTileObstructed(IslandTile tile)
    {
        return tile != null && obstructedTiles.Contains(tile);
    }

    private bool IsWalkable(IslandTile tile)
    {
        return tile != null &&
               tile.owned &&
               tile.tileState != IslandTileState.Wild &&
               tile.tileState != IslandTileState.Clearing &&
               !IsTileObstructed(tile);
    }

    private int GetDistance(IslandTile a, IslandTile b)
    {
        return (
            Mathf.Abs(a.q - b.q) +
            Mathf.Abs(a.r - b.r) +
            Mathf.Abs((a.q + a.r) - (b.q + b.r))
        ) / 2;
    }

    private IslandTile GetLowestScoreTile(
        List<IslandTile> openTiles,
        Dictionary<IslandTile, int> scores)
    {
        IslandTile best = openTiles[0];

        foreach (IslandTile tile in openTiles)
        {
            int tileScore = scores.ContainsKey(tile)
                ? scores[tile]
                : int.MaxValue;

            int bestScore = scores.ContainsKey(best)
                ? scores[best]
                : int.MaxValue;

            if (tileScore < bestScore)
                best = tile;
        }

        return best;
    }

    private void BuildPath(
        Dictionary<IslandTile, IslandTile> cameFrom,
        IslandTile current,
        List<Vector3> path)
    {
        List<IslandTile> reversedPath = new List<IslandTile>();

        while (cameFrom.ContainsKey(current))
        {
            reversedPath.Add(current);
            current = cameFrom[current];
        }

        reversedPath.Reverse();

        foreach (IslandTile tile in reversedPath)
        {
            GameObject visual = tileVisuals.GetTileVisual(tile);

            if (visual == null)
            {
                path.Clear();
                Debug.LogWarning("ISLAND NAVIGATION | Tile visual missing.");
                return;
            }

            path.Add(visual.transform.position);
        }
    }
}
