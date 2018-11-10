using UnityEngine;

public class GroundCollision : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (other.tag == "AxeHead")
        {
            IAxeCollision collision = other.GetComponentInParent<IAxeCollision>();
            collision.OnStuckAt();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "AxeBody")
        {
            IAxeCollision collisionObj = collision.gameObject.GetComponentInParent<IAxeCollision>();
            collisionObj.OnColliderAt();
        }
    }

}
