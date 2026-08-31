using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    void LateUpdate()
    {
        transform.forward =
            Camera.main.transform.forward;
    }
}