
using System.Collections;
using UnityEngine;

public class RockBreakAnimation : MonoBehaviour
{
    [Header("Fragments")]
    public GameObject fragmentPilePrefab;

    [Header("Physics")]
    public float explosionForce = 1f;
    public float upwardForce = 0.5f;
    public float explosionRadius = 1f;
    public float fragmentMass = 1.5f;

    [Header("Ground")]
    public float groundCheckHeight = 5f;
    public float groundOffset = 0.03f;

    [Header("Temporary Ground")]
    public float groundSize = 5f;
    public float groundThickness = 0.2f;
    public float groundDepth = 0.5f;

    private bool hasBroken;

    public void BreakRock()
    {
        if (hasBroken || fragmentPilePrefab == null)
            return;

        hasBroken = true;
        StartCoroutine(BreakAnimation());
    }

    private IEnumerator BreakAnimation()
    {
        Vector3 breakPosition = transform.position;

        // Create temporary ground underneath the rock
        GameObject temporaryGround = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        temporaryGround.name = "Temporary_Rock_Ground";

        temporaryGround.transform.position = new Vector3(
            breakPosition.x,
            breakPosition.y - groundDepth,
            breakPosition.z
        );

        temporaryGround.transform.localScale = new Vector3(
            groundSize,
            groundThickness,
            groundSize
        );

        GameObject pile = Instantiate(
            fragmentPilePrefab,
            breakPosition,
            transform.rotation
        );

        int count = pile.transform.childCount;

        if (count == 0)
        {
            Destroy(pile);
            Destroy(gameObject);
            yield break;
        }

        // Centre the fragment group
        Vector3 centre = Vector3.zero;

        for (int i = 0; i < count; i++)
            centre += pile.transform.GetChild(i).localPosition;

        centre /= count;

        // Prepare individual physical fragments
        for (int i = 0; i < count; i++)
        {
            Transform fragment = pile.transform.GetChild(i);

            fragment.localPosition -= centre;

            MeshCollider meshCollider =
                fragment.GetComponent<MeshCollider>();

            if (meshCollider == null)
                meshCollider = fragment.gameObject.AddComponent<MeshCollider>();

            meshCollider.convex = true;

            Rigidbody rb =
                fragment.GetComponent<Rigidbody>();

            if (rb == null)
                rb = fragment.gameObject.AddComponent<Rigidbody>();

            rb.mass = fragmentMass;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;

            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Slightly different physics material for each fragment
            Collider collider = meshCollider;
            collider.material = CreateRockPhysicsMaterial();

            // Find the ground underneath this fragment
            Vector3 rayOrigin = fragment.position +
                                Vector3.up * groundCheckHeight;

            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                groundCheckHeight * 2f))
            {
                float bottomOffset =
                    collider.bounds.min.y - fragment.position.y;

                fragment.position = new Vector3(
                    fragment.position.x,
                    hit.point.y - bottomOffset + groundOffset,
                    fragment.position.z
                );
            }

            // Scatter fragments using physics
            Vector3 direction =
                (fragment.position - breakPosition).normalized;

            if (direction.sqrMagnitude < 0.01f)
                direction = Random.insideUnitSphere.normalized;

            rb.AddExplosionForce(
                explosionForce,
                breakPosition,
                explosionRadius,
                upwardForce,
                ForceMode.Impulse
            );

            rb.AddForce(
                direction * Random.Range(0.1f, 0.4f),
                ForceMode.Impulse
            );

            rb.AddTorque(
                Random.insideUnitSphere * 0.35f,
                ForceMode.Impulse
            );
        }

        // Hide the original boulder
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        Destroy(gameObject);

        // Allow fragments to settle before removing temporary ground
        yield return new WaitForSeconds(5f);

        Destroy(temporaryGround);
    }

    private PhysicsMaterial CreateRockPhysicsMaterial()
    {
        PhysicsMaterial material = new PhysicsMaterial("Rock_Physics");
        material.dynamicFriction = 0.7f;
        material.staticFriction = 0.8f;
        material.bounciness = 0.12f;
        material.frictionCombine = PhysicsMaterialCombine.Average;
        material.bounceCombine = PhysicsMaterialCombine.Minimum;

        return material;
    }
}
