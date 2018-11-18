using UnityEngine;

public class LazyKinematicBahaviour : MonoBehaviour {

    [SerializeField]
    private float lazyKinematicTime = .5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            Invoke("SetKinematic", lazyKinematicTime);
        }
    }

    private void SetKinematic()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
}
