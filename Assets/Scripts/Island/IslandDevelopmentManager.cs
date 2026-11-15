
using UnityEngine;

public class IslandDevelopmentManager : MonoBehaviour
{
    [Header("Available Developments")]
    [SerializeField]
    private IslandDevelopmentDefinition[] developmentDefinitions;

    private IslandTileVisuals tileVisuals;

    private void Awake()
    {
        tileVisuals = FindFirstObjectByType<IslandTileVisuals>();

        if (tileVisuals == null)
        {
            Debug.LogError(
                "DEVELOPMENT MANAGER | IslandTileVisuals not found."
            );
        }
    }

    public IslandDevelopmentDefinition GetDevelopmentDefinition(
        IslandDevelopmentType type)
    {
        foreach (IslandDevelopmentDefinition definition
                 in developmentDefinitions)
        {
            if (definition != null &&
                definition.developmentType == type)
            {
                return definition;
            }
        }

        return null;
    }

    public bool CanDevelopTile(IslandTile tile)
    {
        if (tile == null)
            return false;

        if (!tile.owned || !tile.isExpansionTile)
            return false;

        if (tile.tileState != IslandTileState.Available)
            return false;

        if (tile.developmentType != IslandDevelopmentType.None)
            return false;

        return true;
    }

    public bool TryDevelopTile(
        IslandTile tile,
        IslandDevelopmentType type)
    {
        if (!CanDevelopTile(tile))
            return false;

        IslandDevelopmentDefinition definition =
            GetDevelopmentDefinition(type);

        if (definition == null)
        {
            Debug.LogWarning(
                "DEVELOPMENT FAILED | Definition not found: " + type
            );
            return false;
        }

        IslandDevelopmentDefinition.DevelopmentLevel level =
            definition.GetLevel(1);

        if (level == null || level.completeTilePrefab == null)
        {
            Debug.LogWarning(
                "DEVELOPMENT FAILED | Level 1 prefab missing: " + type
            );
            return false;
        }

        if (tileVisuals == null)
        {
            Debug.LogError(
                "DEVELOPMENT FAILED | IslandTileVisuals not found."
            );
            return false;
        }

        tile.developmentType = type;
        tile.developmentName = definition.displayName;
        tile.developmentLevel = 1;
        tile.tileState = IslandTileState.Developed;

        bool visualCreated = tileVisuals.ReplaceDevelopmentVisual(
            tile,
            level.completeTilePrefab
        );

        if (!visualCreated)
        {
            tile.developmentType = IslandDevelopmentType.None;
            tile.developmentName = "";
            tile.developmentLevel = 0;
            tile.tileState = IslandTileState.Available;

            Debug.LogWarning(
                "DEVELOPMENT FAILED | Could not create farm visual."
            );

            return false;
        }

        Debug.Log(
            "TILE DEVELOPED | " +
            definition.displayName +
            " | Coordinates: (" + tile.q + ", " + tile.r + ")"
        );

        return true;
    }
}
