
using UnityEngine;

public class IslandTileSelectable : MonoBehaviour
{
    private IslandTile tileData;
    private Renderer[] tileRenderers;
    private MaterialPropertyBlock propertyBlock;
    private bool isSelected;

    private readonly Color unownedColor =
        new Color(0.55f, 0.55f, 0.55f, 1f);

    private readonly Color selectedColor =
        new Color(1f, 0.85f, 0.3f, 1f);

    public IslandTile TileData => tileData;

    private void Awake()
    {
        tileRenderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(IslandTile data)
    {
        tileData = data;
        RefreshRenderers();
        ApplyAppearance();
    }

    public void RefreshRenderers()
    {
        tileRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetSelected(bool selected)
    {
        if (tileData == null)
            return;

        isSelected = selected;

        if (isSelected)
            ApplyColor(selectedColor);
        else
            ApplyAppearance();
    }

    public void RefreshAppearance()
    {
        if (tileData == null)
            return;

        RefreshRenderers();

        if (isSelected)
            ApplyColor(selectedColor);
        else
            ApplyAppearance();
    }

    private void ApplyAppearance()
    {
        if (tileData == null || tileRenderers == null)
            return;

        foreach (Renderer tileRenderer in tileRenderers)
        {
            if (tileRenderer == null)
                continue;

            if (tileData.owned)
            {
                for (int i = 0; i < tileRenderer.sharedMaterials.Length; i++)
                {
                    tileRenderer.SetPropertyBlock(null, i);
                }
            }
            else
            {
                ApplyColorToRenderer(tileRenderer, unownedColor);
            }
        }
    }

    private void ApplyColor(Color color)
    {
        if (tileRenderers == null)
            return;

        foreach (Renderer tileRenderer in tileRenderers)
        {
            if (tileRenderer != null)
                ApplyColorToRenderer(tileRenderer, color);
        }
    }

    private void ApplyColorToRenderer(Renderer renderer, Color color)
    {
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            renderer.GetPropertyBlock(propertyBlock, i);
            propertyBlock.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(propertyBlock, i);
        }
    }
}
