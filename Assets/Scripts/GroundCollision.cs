using UnityEngine;

public class GroundCollision : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AxeHead")
        {
            IAxeCollision collision = other.GetComponentInParent<IAxeCollision>();
            collision.OnStuckAt();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "AxeBody")
        {
            IAxeCollision collisionObj = collision.gameObject.GetComponentInParent<IAxeCollision>();
            collisionObj.OnColliderAt();
        }

        if (collision.collider.tag == "AxeHead")
        {
            IAxeCollision collisionObj = collision.gameObject.GetComponentInParent<IAxeCollision>();
            collisionObj.OnStuckAt();
        }
    }

}
