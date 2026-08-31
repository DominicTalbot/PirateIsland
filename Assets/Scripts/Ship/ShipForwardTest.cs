using UnityEngine;

public class ShipForwardTest : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.Translate(
            Vector3.forward * speed * Time.deltaTime,
            Space.Self);
    }
}