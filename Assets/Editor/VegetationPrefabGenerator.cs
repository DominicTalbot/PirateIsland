
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class VegetationPrefabGenerator
{
    private const string SavePath =
        "Assets/Prefabs/Obstacles/Vegetation";

    private static readonly string[] VegetationNames =
    {
        "Bush_01",
        "Bush_02",
        "Bush_03",
        "Tall_Weeds_01",
        "Tall_Weeds_02",
        "Tall_Weeds_03",
        "Small_Plant_01",
        "Small_Plant_02",
        "Small_Plant_03"
    };

    [MenuItem("Pirate Island/Create Vegetation Prefabs")]
    public static void CreatePrefabs()
    {
        Directory.CreateDirectory(SavePath);

        int created = 0;

        foreach (string vegetationName in VegetationNames)
        {
            // Find all scene objects belonging to this variation
            List<GameObject> parts =
                Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj =>
                    obj.scene.IsValid() &&
                    !EditorUtility.IsPersistent(obj) &&
                    obj.name.StartsWith(vegetationName + "_"))
                .ToList();

            if (parts.Count == 0)
            {
                Debug.LogWarning(
                    "No foliage pieces found for: " + vegetationName
                );
                continue;
            }

            // Calculate the centre of this vegetation model
            Bounds bounds = new Bounds(
                parts[0].transform.position,
                Vector3.zero
            );

            foreach (GameObject part in parts)
            {
                Renderer renderer = part.GetComponent<Renderer>();

                if (renderer != null)
                    bounds.Encapsulate(renderer.bounds);
            }

            // Create a root at the model's centre
            GameObject root = new GameObject(vegetationName);
            root.transform.position = bounds.center;

            // Copy every foliage piece into the root
            foreach (GameObject part in parts)
            {
                GameObject copy = Object.Instantiate(part);
                copy.name = part.name;

                copy.transform.SetParent(
                    root.transform,
                    true
                );
            }

            string prefabPath =
                SavePath + "/" + vegetationName + ".prefab";

            PrefabUtility.SaveAsPrefabAsset(
                root,
                prefabPath
            );

            Object.DestroyImmediate(root);

            Debug.Log(
                "CREATED: " + vegetationName +
                " | Foliage pieces: " + parts.Count
            );

            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "VEGETATION PREFABS CREATED: " + created + "/9"
        );
    }
}
