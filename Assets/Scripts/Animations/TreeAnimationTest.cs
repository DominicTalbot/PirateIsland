
using UnityEngine;

public class TreeAnimationTest : MonoBehaviour
{
    private TreeChopAnimation treeAnimation;

    private void Start()
    {
        treeAnimation = GetComponent<TreeChopAnimation>();

        if (treeAnimation == null)
        {
            Debug.LogError("TREE TEST | TreeChopAnimation missing!");
        }
    }

    private void Update()
    {
        if (treeAnimation == null)
            return;

        // Press T to simulate an axe hit.
        if (Input.GetKeyDown(KeyCode.T))
        {
            treeAnimation.PlayChopImpact();
        }

        // Press F to make the tree fall.
        if (Input.GetKeyDown(KeyCode.F))
        {
            treeAnimation.Fall(Vector3.right);
        }
    }
}
