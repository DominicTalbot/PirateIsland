using UnityEngine;

public class ViewShipButton : MonoBehaviour
{
    public string shipId = "ship_001";

    public void ViewShip()
    {
        if (
            SceneNavigator.Instance == null
        )
        {
            Debug.LogError(
                "SceneNavigator not found."
            );

            return;
        }

        SceneNavigator.Instance
            .GoToVoyage(shipId);
    }
}