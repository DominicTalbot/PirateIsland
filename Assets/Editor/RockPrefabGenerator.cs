
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class RockPrefabGenerator
{
    private const string SavePath =
        "Assets/Prefabs/Obstacles/Rocks";

    [MenuItem("Pirate Island/Create Rock Prefabs")]
    public static void CreateRockPrefabs()
    {
        string[] rockNames =
        {
            "Rock_Small_01",
            "Rock_Small_02",
            "Rock_Medium_01",
            "Rock_Medium_02",
            "Rock_Large_01"
        };

        Directory.CreateDirectory(SavePath);

        int created = 0;

        foreach (string rockName in rockNames)
        {
            GameObject rock = FindSceneObject(rockName);

            if (rock == null)
            {
                Debug.LogWarning("Could not find: " + rockName);
                continue;
            }

            PrefabUtility.SaveAsPrefabAsset(
                rock,
                SavePath + "/" + rockName + ".prefab"
            );

            created++;
        }

        // Create a combined fragment pile
        string[] fragmentNames =
        {
            "Rock_Fragment_01",
            "Rock_Fragment_02",
            "Rock_Fragment_03",
            "Rock_Fragment_04"
        };

        GameObject pile = new GameObject("Rock_Fragment_Pile");

        int fragmentCount = 0;

        foreach (string fragmentName in fragmentNames)
        {
            GameObject fragment = FindSceneObject(fragmentName);

            if (fragment == null)
            {
                Debug.LogWarning("Could not find: " + fragmentName);
                continue;
            }

            GameObject copy = Object.Instantiate(fragment);
            copy.name = fragment.name;
            copy.transform.SetParent(pile.transform, true);

            fragmentCount++;
        }

        if (fragmentCount > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(
                pile,
                SavePath + "/Rock_Fragment_Pile.prefab"
            );

            created++;
        }

        Object.DestroyImmediate(pile);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("ROCK PREFABS CREATED: " + created);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(obj =>
                obj.name == objectName &&
                obj.scene.IsValid() &&
                !EditorUtility.IsPersistent(obj));
    }
}
