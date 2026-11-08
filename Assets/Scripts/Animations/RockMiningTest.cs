
using UnityEngine;

public class RockMiningTest : MonoBehaviour
{
    private RockMiningAnimation rockAnimation;
    private RockBreakAnimation breakAnimation;

    private void Awake()
    {
        rockAnimation = GetComponent<RockMiningAnimation>();
        breakAnimation = GetComponent<RockBreakAnimation>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (rockAnimation != null)
                rockAnimation.PlayImpact();

            Debug.Log("ROCK HIT");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (breakAnimation != null)
                breakAnimation.BreakRock();

            Debug.Log("ROCK BREAK TEST");
        }
    }
}
