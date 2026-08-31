using UnityEngine;

public class ReturnToIslandButton : MonoBehaviour
{
    public void ReturnToIsland()
    {
        if (SceneNavigator.Instance != null)
        {
            SceneNavigator.Instance.GoToIsland();
        }
        else
        {
            Debug.LogError(
                "SceneNavigator.Instance is missing!"
            );
        }
    }
}