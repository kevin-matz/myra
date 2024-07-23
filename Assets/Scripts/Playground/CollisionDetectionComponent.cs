using UnityEngine;

public class CollisionDetectionComponent : MonoBehaviour
{
    /* Everything except post it notes and the ground plane */
    private const int layerMask = ~((1 << 7) | (1 << 6));

    public bool IsColliding;

    private void Start()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        var collider = gameObject.GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) > 0)
        {
            IsColliding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) > 0)
        {
            IsColliding = false;
        }
    }
}
