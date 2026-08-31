using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(
                "CLICK DETECTED | PointerOverUI: " +
                EventSystem.current.IsPointerOverGameObject()
            );

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log(
                    "CLICK BLOCKED BY UI"
                );

                return;
            }

            Ray ray =
                Camera.main.ScreenPointToRay(
                    Input.mousePosition
                );

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(
                    "WORLD HIT: " +
                    hit.collider.name
                );

                BuildingClickable building =
                    hit.collider.GetComponentInParent<BuildingClickable>();

                if (building != null)
                {
                    Debug.Log(
                        "BUILDING CLICKED: " +
                        building.buildingName
                    );

                    UIManager.Instance
                        .OpenBuildingPanel(
                            building
                        );
                }
            }
            else
            {
                Debug.Log(
                    "CLICK HIT NOTHING"
                );
            }
        }
    }
}