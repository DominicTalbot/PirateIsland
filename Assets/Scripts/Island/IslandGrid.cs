using UnityEngine;

public class IslandGrid : MonoBehaviour
{

    [SerializeField]
    private IslandDataManager dataManager;

    [Header("Grid Settings")]
    public int radius = 4;

    [Header("Tile Settings")]
    public float tileSize = 3f;

    private void Awake()
    {
        if (dataManager == null)
        {
            dataManager =
                GetComponent<IslandDataManager>();
        }
    }

    private void OnDrawGizmos()
    {
        if (dataManager == null)
        {
            return;
        }

        foreach (IslandTile tile in dataManager.Tiles)
        {
            if (!tile.owned)
            {
                continue;
            }

            Vector3 center =
                GetTileWorldPosition(tile.q, tile.r);

            float radius = tileSize;

            Vector3[] corners = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                float angle =
                    Mathf.Deg2Rad * (60f * i);

                corners[i] =
                    center +
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius
                    );
            }

            for (int edge = 0; edge < 6; edge++)
            {
                int next = (edge + 1) % 6;

                if (tile.edges[edge].edgeType ==
                    IslandEdgeType.Ocean)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawLine(
                    corners[edge],
                    corners[next]
                );
            }
        }
    }

    private Vector3 GetTileWorldPosition(int q, int r)
    {
        float x =
            tileSize *
            1.5f *
            q;

        float z =
            tileSize *
            Mathf.Sqrt(3f) *
            (r + q * 0.5f);

        return transform.position +
               new Vector3(x, 0f, z);
    }
}