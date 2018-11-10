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

    [Header ("Axe")]
    [SerializeField]
    private Transform axeMeshTransform;
    [SerializeField]
    private Transform massCenterTransform;

    [Header("Powerful Hand")]
    [SerializeField]
    private Transform powerfulHandTransform;

    private AxeState axeState;
    private AxeGrabbable axeGrabbable;

    private Rigidbody rb;

    private bool isFirstGrab = true;
    private Quaternion axeGrabRotation;

    private float rotationSpeedWhenTravelling = -720;
    private float rotationSpeedWhenReturning = 360;

    private bool isAvailableToReturn = false;
    private float remainingTimeToReturn;
    private float timeToReturning = 1f;

    private bool isThrowToClose = false;
    private float throwThreshold = 12f;
    private float returningStartTime;
    private float returningJourneyLength;
    private float returningArcZ = 10f;
    private Vector3 returningStartPosition;
    private Vector3 returningMiddlePosition;

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

        if (powerfulHandTransform == null)
        {
            throw new Exception("Should have an Powerfull Hand");
        }
    }

    private void OnEnable()
    {
        axeGrabbable.OnAxeGrabbableAction += GrabbableAxe;
        axeGrabbable.OnAxeDroppedAction += DropAxe;
        axeGrabbable.OnAxeTrownAction += ThrownAxe;

        PowerfulHandBehaviour powerBehaviour = powerfulHandTransform.GetComponent<PowerfulHandBehaviour>();
        powerBehaviour.OnPowerHandEvent += OnAxeCalledEvent;
    }

    private void Update()
    {
        switch (axeState)
        {
            case AxeState.Dropped:
                UpdateRemainingTime();
                break;
            case AxeState.Travelling:
                UpdateRemainingTime();
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

        if (powerfulHandTransform != null)
        {
            PowerfulHandBehaviour powerBehaviour = powerfulHandTransform.GetComponent<PowerfulHandBehaviour>();
            powerBehaviour.OnPowerHandEvent -= OnAxeCalledEvent;
        }

    }

    public void OnAxeCalledEvent()
    {
        if ((axeState == AxeState.Dropped 
            || axeState == AxeState.Stucked 
            || axeState == AxeState.Travelling)
            && isAvailableToReturn)
        {
            OnAxeCalled();
        }
    }

    public void OnStuckAt()
    {
        if (axeState != AxeState.Returning)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            isAvailableToReturn = true;
            axeState = AxeState.Stucked;
        }
    }

    public void OnColliderAt()
    {
        if (axeState != AxeState.Dropped)
        {
            DropAxe();
        }  
    }

    private void GrabbableAxe()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        isAvailableToReturn = false;

        axeMeshTransform.rotation = new Quaternion(0, 0, 0, 0);

        axeState = AxeState.Grabbled;
    }

    private void DropAxe()
    {
        remainingTimeToReturn = timeToReturning;
        Physics.gravity = normalGravity;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;

        axeState = AxeState.Dropped;
    }

    private void ThrownAxe(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        remainingTimeToReturn = timeToReturning;
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

    private void UpdateRemainingTime()
    {
        remainingTimeToReturn -= Time.deltaTime;
        if (remainingTimeToReturn <= 0)
        {
            isAvailableToReturn = true;
        }
    }

    private void OnAxeCalled()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        returningStartTime = Time.time;
        returningStartPosition = transform.position;

        returningJourneyLength = Vector3.Distance(returningStartPosition, powerfulHandTransform.position);

        if (returningJourneyLength < throwThreshold)
        {
            isThrowToClose = true;
        }
        else
        {
            isThrowToClose = false;
            if (transform.forward.z < 0)
            {
                returningMiddlePosition = returningStartPosition + (powerfulHandTransform.position - returningStartPosition) / 2 + (-Vector3.forward * returningArcZ);
            }
            else
            {
                returningMiddlePosition = returningStartPosition + (powerfulHandTransform.position - returningStartPosition) / 2 + (Vector3.forward * returningArcZ);
            }
        }

        axeState = AxeState.Returning;
    }

    private void OnAxeReturning()
    {
        float distCovered = (Time.time - returningStartTime) * returningJourneyLength;
        float fracJourney = distCovered / returningJourneyLength;

        if (isThrowToClose)
        {
            transform.position = Vector3.Lerp(returningStartPosition, powerfulHandTransform.position, fracJourney * 2);
        }
        else
        {
            Vector3 point1 = Vector3.Lerp(returningStartPosition, returningMiddlePosition, fracJourney);
            Vector3 point2 = Vector3.Lerp(returningMiddlePosition, powerfulHandTransform.position, fracJourney);

            transform.position = Vector3.Lerp(point1, point2, fracJourney);
        }

        axeMeshTransform.Rotate(0, 0, rotationSpeedWhenReturning * Time.deltaTime, Space.Self);
    }
}
