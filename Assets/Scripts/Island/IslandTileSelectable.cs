
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class IslandTileSelectable : MonoBehaviour
{
    private IslandTile tileData;
    private Renderer tileRenderer;
    private MaterialPropertyBlock propertyBlock;

    public IslandTile TileData => tileData;

    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(IslandTile data)
    {
        tileData = data;
    }

    public void SetSelected(bool selected)
    {
        if (tileRenderer == null)
            return;

        for (int i = 0; i < tileRenderer.sharedMaterials.Length; i++)
        {
            if (selected)
            {
                tileRenderer.GetPropertyBlock(propertyBlock, i);

                propertyBlock.SetColor(
                    "_BaseColor",
                    new Color(1f, 0.85f, 0.3f, 1f)
                );

                tileRenderer.SetPropertyBlock(propertyBlock, i);
            }
            else
            {
                tileRenderer.SetPropertyBlock(null, i);
            }
        }
    }
}
