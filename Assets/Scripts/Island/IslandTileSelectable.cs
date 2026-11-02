
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class IslandTileSelectable : MonoBehaviour
{
    private IslandTile tileData;
    private Renderer tileRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool isSelected;

    private readonly Color unownedColor =
        new Color(0.55f, 0.55f, 0.55f, 1f);

    private readonly Color selectedColor =
        new Color(1f, 0.85f, 0.3f, 1f);

    public IslandTile TileData => tileData;

    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(IslandTile data)
    {
        tileData = data;
        ApplyAppearance();
    }

    public void SetSelected(bool selected)
    {
        if (tileRenderer == null || tileData == null)
            return;

        isSelected = selected;

        if (isSelected)
            ApplyColor(selectedColor);
        else
            ApplyAppearance();
    }

    public void RefreshAppearance()
    {
        if (tileRenderer == null || tileData == null)
            return;

        if (isSelected)
            ApplyColor(selectedColor);
        else
            ApplyAppearance();
    }

    private void ApplyAppearance()
    {
        if (tileData == null || tileRenderer == null)
            return;

        if (tileData.owned)
        {
            // Restore the original green and brown prefab materials.
            for (int i = 0; i < tileRenderer.sharedMaterials.Length; i++)
            {
                tileRenderer.SetPropertyBlock(null, i);
            }
        }
        else
        {
            ApplyColor(unownedColor);
        }
    }

    private void ApplyColor(Color color)
    {
        for (int i = 0; i < tileRenderer.sharedMaterials.Length; i++)
        {
            tileRenderer.GetPropertyBlock(propertyBlock, i);
            propertyBlock.SetColor("_BaseColor", color);
            tileRenderer.SetPropertyBlock(propertyBlock, i);
        }
    }
}
