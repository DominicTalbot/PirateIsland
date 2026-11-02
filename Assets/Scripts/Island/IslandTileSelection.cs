
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandTileSelection : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float rayDistance = 1000f;

    private IslandTileSelectable selectedTile;

    // Notifies the UI whenever the selected tile changes.
    public event Action<IslandTile> SelectionChanged;
    public event Action<Transform> TileTransformSelected;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        SelectTileUnderMouse();
    }

    private void SelectTileUnderMouse()
    {
        if (mainCamera == null)
        {
            Debug.LogError("ISLAND SELECTION | Main camera missing.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            ClearSelection();
            return;
        }

        IslandTileSelectable tile =
            hit.collider.GetComponentInParent<IslandTileSelectable>();

        if (tile == null)
        {
            ClearSelection();
            return;
        }

        if (selectedTile == tile)
            return;

        ClearSelection();

        selectedTile = tile;
        selectedTile.SetSelected(true);

        TileTransformSelected?.Invoke(selectedTile.transform);

        IslandTile data = selectedTile.TileData;

        if (data != null)
        {
            Debug.Log(
                $"ISLAND TILE SELECTED | " +
                $"Coordinates: ({data.q}, {data.r}) | " +
                $"Terrain: {data.terrainType} | " +
                $"Owned: {data.owned} | " +
                $"State: {data.tileState}"
            );

            SelectionChanged?.Invoke(data);
        }
    }

    public void ClearSelection()
    {
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false);
            selectedTile = null;
        }

        SelectionChanged?.Invoke(null);
        TileTransformSelected?.Invoke(null);
    }

    public void RefreshSelectedTileAppearance()
    {
        if (selectedTile != null)
            selectedTile.RefreshAppearance();
    }
}
