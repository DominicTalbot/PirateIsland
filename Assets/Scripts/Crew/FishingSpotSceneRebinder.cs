using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FishingSpotSceneRebinder
{
    private static bool installed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (installed)
        {
            return;
        }

        installed = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (scene.name != "IslandScene")
        {
            return;
        }

        RebindFishingSpots();
    }

    private static void RebindFishingSpots()
    {
        if (CrewManager.Instance == null)
        {
            Debug.LogWarning(
                "FISHING SPOT REBIND SKIPPED - CrewManager not ready."
            );

            return;
        }

        Scene islandScene =
            SceneManager.GetActiveScene();

        if (islandScene.name != "IslandScene")
        {
            return;
        }

        List<Transform> spots =
            new List<Transform>();

        GameObject[] roots =
            islandScene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            CollectFishingSpots(
                root.transform,
                spots
            );
        }

        spots.Sort(CompareFishingSpots);

        CrewManager.Instance.fishingSpots =
            spots.ToArray();

        Debug.Log(
            "FISHING SPOTS REBOUND | Count: " +
            spots.Count
        );

        for (int i = 0; i < spots.Count; i++)
        {
            Debug.Log(
                "FISHING SPOT " +
                i +
                " -> " +
                spots[i].name
            );
        }

        if (spots.Count == 0)
        {
            Debug.LogWarning(
                "NO FISHING SPOTS FOUND IN ISLAND SCENE. " +
                "Make sure their GameObject names contain 'Fishing' and 'Spot' " +
                "or 'Point'."
            );
        }
    }

    private static void CollectFishingSpots(
        Transform current,
        List<Transform> results
    )
    {
        if (current == null)
        {
            return;
        }

        string lowerName =
            current.name.ToLowerInvariant();

        bool looksLikeFishingSpot =
            lowerName.Contains("fishing") &&
            (
                lowerName.Contains("spot") ||
                lowerName.Contains("point")
            );

        if (looksLikeFishingSpot)
        {
            results.Add(current);
        }

        for (int i = 0; i < current.childCount; i++)
        {
            CollectFishingSpots(
                current.GetChild(i),
                results
            );
        }
    }

    private static int CompareFishingSpots(
        Transform a,
        Transform b
    )
    {
        int aNumber =
            GetTrailingNumber(a.name);

        int bNumber =
            GetTrailingNumber(b.name);

        bool aHasNumber =
            aNumber >= 0;

        bool bHasNumber =
            bNumber >= 0;

        if (aHasNumber && bHasNumber)
        {
            int numberCompare =
                aNumber.CompareTo(bNumber);

            if (numberCompare != 0)
            {
                return numberCompare;
            }
        }
        else if (aHasNumber != bHasNumber)
        {
            return aHasNumber ? -1 : 1;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(
            a.name,
            b.name
        );
    }

    private static int GetTrailingNumber(
        string value
    )
    {
        if (string.IsNullOrEmpty(value))
        {
            return -1;
        }

        int end =
            value.Length - 1;

        while (end >= 0 && char.IsDigit(value[end]))
        {
            end--;
        }

        if (end == value.Length - 1)
        {
            return -1;
        }

        string numberText =
            value.Substring(end + 1);

        if (int.TryParse(numberText, out int number))
        {
            return number;
        }

        return -1;
    }
}
