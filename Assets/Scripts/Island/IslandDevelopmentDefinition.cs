
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Island Development",
    menuName = "Pirate Island/Island Development"
)]
public class IslandDevelopmentDefinition : ScriptableObject
{
    [System.Serializable]
    public class DevelopmentLevel
    {
        public int level = 1;
        public GameObject completeTilePrefab;

        [Header("Upgrade Cost")]
        public int goldCost;
    }

    [Header("Development")]
    public IslandDevelopmentType developmentType;
    public string displayName;
    [TextArea]
    public string description;

    [Header("Levels")]
    public DevelopmentLevel[] levels;

    public DevelopmentLevel GetLevel(int level)
    {
        foreach (DevelopmentLevel developmentLevel in levels)
        {
            if (developmentLevel.level == level)
                return developmentLevel;
        }

        return null;
    }
}
