using System;

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

    // Development
    public string developmentName;
    public int developmentLevel;

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

        owned = false;

        developmentName = "";

        developmentLevel = 0;

        edges = new IslandTileEdge[6];

        for (int i = 0; i < 6; i++)
        {
            edges[i] = new IslandTileEdge(i);
        }
    }
}