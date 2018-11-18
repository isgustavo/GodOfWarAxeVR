using UnityEngine;

public class BreakableBehaviour : MonoBehaviour {

    [SerializeField]
    private GameObject breakableObject;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnCollision(other);
    }


    private void OnCollision(Collider collider)
    {
        if (collider.tag == "AxeHead" || collider.tag == "AxeBody")
        {
            Instantiate(breakableObject, transform.position, transform.rotation);
            gameObject.SetActive(false);
        }
    }

}
