using UnityEngine;

public class VoyageCameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset =
        new Vector3(0, 8, -10);

    void LateUpdate()
    {
        if (target == null)
            return;

        transform.position =
            target.position + offset;

        transform.LookAt(target);
    }
}