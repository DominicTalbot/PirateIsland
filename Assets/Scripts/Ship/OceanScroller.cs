using UnityEngine;

public class OceanScroller : MonoBehaviour
{
    public float speed = 0.05f;

    private Renderer rend;

    private void Start()
    {
        rend =
            GetComponent<Renderer>();
    }

    private void Update()
    {
        Vector2 offset =
            new Vector2(
                0,
                Time.time * speed
            );

        rend.material
            .mainTextureOffset =
            offset;
    }
}