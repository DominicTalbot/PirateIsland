
using UnityEngine;

public class VegetationClearTest : MonoBehaviour
{
    private VegetationClearAnimation clearAnimation;

    private void Awake()
    {
        clearAnimation =
            GetComponent<VegetationClearAnimation>();

        if (clearAnimation == null)
        {
            Debug.LogError(
                "VEGETATION TEST | Missing VegetationClearAnimation!",
                this
            );
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (clearAnimation == null)
                return;

            clearAnimation.ClearVegetation();

            Debug.Log("VEGETATION CLEARING TEST");
        }
    }
}
