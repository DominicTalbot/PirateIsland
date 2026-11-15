
using UnityEngine;

public class IslandObstacleVisual : MonoBehaviour
{
    public IslandTile Tile { get; private set; }
    public IslandObstacle Obstacle { get; private set; }

    public void Initialize(IslandTile tile, IslandObstacle obstacle)
    {
        Tile = tile;
        Obstacle = obstacle;
    }
}
