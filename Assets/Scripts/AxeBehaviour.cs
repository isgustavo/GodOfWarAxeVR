using System;
using UnityEngine;

public interface IAxeCollision 
{
    void OnStuckAt();
    void OnColliderAt();
}

[RequireComponent (typeof(AxeGrabbable))]
public class AxeBehaviour : MonoBehaviour, IAxeCollision
{

    public enum AxeState
    {
        None,
        Grabbled,
        Dropped,
        Travelling,
        Stucked,
        Returning
    }

    [SerializeField]
    private Transform axeMeshTransform;
    [SerializeField]
    private Transform massCenterTransform;

    private AxeState axeState;
    private AxeGrabbable axeGrabbable;

    private Rigidbody rb;

    private float rotationSpeedWhenTravelling = -720;
    private float rotationSpeedWhenReturning = 360;

    private Vector3 normalGravity = new Vector3(0f, -9.81f, 0f);
    private Vector3 travellingGravity = new Vector3(0f, -2.81f, 0f);

    private void Awake()
    {
        axeState = AxeState.None;
        axeGrabbable = GetComponent<AxeGrabbable>();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenterTransform.position;

        if (axeMeshTransform == null)
        {
            throw new Exception("Should have an Axe Mesh Transform");
        }
    }

    private void OnEnable()
    {
        axeGrabbable.OnAxeDroppedAction += DropAxe;
        axeGrabbable.OnAxeTrownAction += ThrownAxe;
    }

    private void Update()
    {
        switch (axeState)
        {
            case AxeState.Travelling:
                OnAxeTravelling();
                break;
            case AxeState.Returning:
                OnAxeReturning();
                break;
        }
    }

    private void OnDisable()
    {
        axeGrabbable.OnAxeDroppedAction -= DropAxe;
        axeGrabbable.OnAxeTrownAction -= ThrownAxe;
    }

    public void OnStuckAt()
    {
        rb.useGravity = false;
        rb.isKinematic = true;

        axeState = AxeState.Stucked;
    }

    public void OnColliderAt()
    {
        rb.useGravity = true;
        rb.isKinematic = false;

        axeState = AxeState.None;
    }

    private void DropAxe()
    {
        Physics.gravity = normalGravity;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;

        axeState = AxeState.Dropped;
    }

    private void ThrownAxe(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        rb.velocity = linearVelocity * 2;
        rb.angularVelocity = angularVelocity;
        Physics.gravity = travellingGravity;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;
        rb.isKinematic = false;

        axeState = AxeState.Travelling;
    }

    private void OnAxeTravelling()
    {
        axeMeshTransform.Rotate(0, 0, rotationSpeedWhenTravelling * Time.deltaTime, Space.Self);
    }

    private void OnAxeReturning()
    {

    }
}
