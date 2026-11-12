using System;
using System.Collections.Generic;

public enum IslandTerrainType
{
    Grass,
    Forest,
    Mountain,
    Beach,
    Water
}

public enum IslandTileState
{
    Wild,
    Clearing,
    Available,
    UnderConstruction,
    Developed
}

public enum IslandEdgeType
{
    Land,
    Ocean,
    Lake
}

public enum IslandObstacleType
{
    Tree,
    Rock,
    Vegetation,
    Debris
}


public enum IslandDevelopmentType
{
    None,
    Farm,
    LumberCamp,
    Fishing,
    TrainingGround,
    Storage,
    Housing
}


[Serializable]
public class IslandObstacle
{
    public IslandObstacleType obstacleType;

    // Position relative to the centre of its tile
    public UnityEngine.Vector3 localPosition;

    public float workRequired;
    public float workRemaining;

    public IslandObstacle(
        IslandObstacleType type,
        UnityEngine.Vector3 position,
        float work
    )
    {
        obstacleType = type;
        localPosition = position;
        workRequired = work;
        workRemaining = work;
    }
}

[Serializable]
public class IslandTileEdge
{
    public int edgeIndex;
    public IslandEdgeType edgeType;

    public IslandTileEdge(int index)
    {
        edgeIndex = index;
        edgeType = IslandEdgeType.Land;
    }
}

[Serializable]
public class IslandTile
{
    // Hex-grid coordinates
    public int q;
    public int r;

    // Terrain
    public IslandTerrainType terrainType;

    // Tile state
    public IslandTileState tileState;

    // Ownership
    public bool owned;
    public bool isExpansionTile;

    // Development
    // Development
    public IslandDevelopmentType developmentType;
    public string developmentName;
    public int developmentLevel;
    // Obstacles that need clearing from this tile
    public List<IslandObstacle> obstacles;

    // Six edges around this hex
    public IslandTileEdge[] edges;

    public IslandTile(
        int q,
        int r,
        IslandTerrainType terrainType
    )
    {
        this.q = q;
        this.r = r;

        this.terrainType = terrainType;

        tileState = IslandTileState.Wild;

        obstacles = new List<IslandObstacle>();

        owned = false;
        isExpansionTile = false;

        developmentType = IslandDevelopmentType.None;
        developmentName = "";
        developmentLevel = 0;

        edges = new IslandTileEdge[6];

        for (int i = 0; i < 6; i++)
        {
            edges[i] = new IslandTileEdge(i);
        }
    }
}