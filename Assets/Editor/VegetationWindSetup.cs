
using UnityEngine;
using UnityEditor;
using System.IO;

public static class VegetationWindSetup
{
    private const string PrefabFolder =
        "Assets/Prefabs/Obstacles/Vegetation";

    [MenuItem("Pirate Island/Apply Wind To All Vegetation")]
    public static void ApplyWind()
    {
        string[] prefabGuids =
            AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });

        int updated = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab =
                PrefabUtility.LoadPrefabContents(path);

            if (prefab.GetComponent<VegetationWindAnimation>() == null)
            {
                prefab.AddComponent<VegetationWindAnimation>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            PrefabUtility.UnloadPrefabContents(prefab);

            updated++;
        }

        AssetDatabase.Refresh();

        Debug.Log("VEGETATION WIND SETUP COMPLETE: " + updated);
    }
}
