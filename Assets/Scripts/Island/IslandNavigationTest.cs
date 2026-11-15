
using System.Collections.Generic;
using UnityEngine;

public class IslandNavigationTest : MonoBehaviour
{
    [SerializeField] private IslandNavigation navigation;
    [SerializeField] private IslandDataManager dataManager;

    [Header("Test Coordinates")]
    [SerializeField] private int startQ = 0;
    [SerializeField] private int startR = 0;
    [SerializeField] private int destinationQ = 2;
    [SerializeField] private int destinationR = 0;

    [Header("Blocked Tile")]
    [SerializeField] private int blockedQ = 1;
    [SerializeField] private int blockedR = 0;

    [ContextMenu("Test Dynamic Obstruction")]
    public void TestDynamicObstruction()
    {
        if (navigation == null || dataManager == null)
        {
            Debug.LogError("ROUTE TEST | Navigation or Data Manager missing.");
            return;
        }

        IslandTile start = dataManager.GetTile(startQ, startR);
        IslandTile destination = dataManager.GetTile(destinationQ, destinationR);
        IslandTile blocked = dataManager.GetTile(blockedQ, blockedR);

        if (start == null || destination == null || blocked == null)
        {
            Debug.LogError("ROUTE TEST | One or more test tiles could not be found.");
            return;
        }

        bool wasAlreadyObstructed = navigation.IsTileObstructed(blocked);

        try
        {
            navigation.SetTileObstructed(blocked, true);

            Debug.Log(
                "ROUTE TEST | Obstructed tile (" +
                blockedQ + ", " + blockedR + ")"
            );

            if (navigation.TryFindPath(start, destination, out List<Vector3> path))
            {
                if (path.Count > 2)
                {
                    Debug.Log("DYNAMIC OBSTRUCTION SUCCESS | Steps: " + path.Count);

                    foreach (Vector3 point in path)
                        Debug.Log("DYNAMIC ROUTE POINT | " + point);
                }
                else
                {
                    Debug.LogWarning(
                        "DYNAMIC OBSTRUCTION TEST | Route did not take an alternative path."
                    );
                }
            }
            else
            {
                Debug.LogError("DYNAMIC OBSTRUCTION FAILED | No alternative route found.");
            }
        }
        finally
        {
            navigation.SetTileObstructed(blocked, wasAlreadyObstructed);

            Debug.Log("ROUTE TEST | Original obstruction state restored.");
        }
    }
}
