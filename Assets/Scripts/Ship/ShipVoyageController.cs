using UnityEngine;

public class ShipVoyageController : MonoBehaviour
{
    public Transform destination;

    public float speed = 5f;

    void Update()
    {
        transform.Translate(
            Vector3.forward *
            speed *
            Time.deltaTime,
            Space.Self
        );
    }
}