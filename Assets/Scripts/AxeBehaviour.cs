using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IAxeCollision 
{
    void OnStuckAt();
    void OnColliderAt();
}

public class AudioEvent : UnityEvent<AudioClip> { }

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

    [Header("Axe audio clips")]
    [SerializeField]
    private AudioClip axeAudioClip;
    [SerializeField]
    private AudioClip grabAudioClip;
    [SerializeField]
    private AudioClip returningAudioClip;
    [SerializeField]
    private AudioClip collisionAudioClip;
    [SerializeField]
    private AudioClip wrongCollisionAudioClip;

    [Header("Powerful Hand")]
    [SerializeField]
    private Transform handTransform;

    private AxeState axeState;
    private AxeGrabbable axeGrabbable;
    private AxeAudioBehaviour audioBehaviour;

    private Rigidbody rb;

    private bool isFirstGrab = true;
    private Quaternion axeGrabRotation;

    private readonly float rotationSpeedWhenTravelling = -720;
    private readonly float rotationSpeedWhenReturning = 360;

    private bool isAvailableToReturn = false;
    private float remainingTimeToReturn;
    private readonly float timeToReturning = 1f;

    private bool isThrowToClose = false;
    private readonly float throwThreshold = 12f;
    private float returningStartTime;
    private float returningJourneyLength;
    private readonly float returningArcZ = 10f;
    private Vector3 returningStartPosition;
    private Vector3 returningMiddlePosition;

    private readonly Vector3 normalGravity = new Vector3(0f, -9.81f, 0f);
    private readonly Vector3 travellingGravity = new Vector3(0f, -2.81f, 0f);

    private List<Collider> colliderTrigabbles = new List<Collider>();

    private AudioEvent OnPlayOneShotEvent = new AudioEvent();
    private AudioEvent OnPlayLoopEvent = new AudioEvent();

    private UnityEvent OnGrabbableEvent = new UnityEvent();
    private UnityEvent OnReturningEvent = new UnityEvent();

    private void Awake()
    {
        axeState = AxeState.None;
        axeGrabbable = GetComponent<AxeGrabbable>();
        audioBehaviour = GetComponent<AxeAudioBehaviour>();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = massCenterTransform.position;

        if (axeMeshTransform == null)
        {
            throw new Exception("Should have an Axe Mesh Transform");
        }

        if (handTransform == null)
        {
            throw new Exception("Should have an Powerfull Hand");
        }

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject.layer == 10)
            {
                colliderTrigabbles.Add(collider);
            }
        }
    }

    private void OnEnable()
    {
        axeGrabbable.OnAxeGrabbableAction += GrabbableAxe;
        axeGrabbable.OnAxeDroppedAction += DropAxe;
        axeGrabbable.OnAxeTrownAction += ThrownAxe;

        PowerfulHandBehaviour powerBehaviour = handTransform.GetComponent<PowerfulHandBehaviour>();
        powerBehaviour.OnPowerHandEvent += OnAxeCalledEvent;

        HandVibrateBehaviour handBehaviour = handTransform.GetComponent<HandVibrateBehaviour>();
        OnGrabbableEvent.AddListener(handBehaviour.Vibrate);
        OnReturningEvent.AddListener(handBehaviour.VibrateNonStop);

        OnPlayOneShotEvent.AddListener(audioBehaviour.PlayOneShot);
        OnPlayLoopEvent.AddListener(audioBehaviour.PlayLoop);
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

        if (handTransform != null)
        {
            PowerfulHandBehaviour powerBehaviour = handTransform.GetComponent<PowerfulHandBehaviour>();
            powerBehaviour.OnPowerHandEvent -= OnAxeCalledEvent;
        }

        OnGrabbableEvent.RemoveAllListeners();
        OnReturningEvent.RemoveAllListeners();

        OnPlayOneShotEvent.RemoveAllListeners();
        OnPlayLoopEvent.RemoveAllListeners();
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

            OnPlayOneShotEvent.Invoke(collisionAudioClip);
            axeState = AxeState.Stucked;
        }
    }

    public void OnColliderAt()
    {
        if (axeState != AxeState.Dropped)
        {
            OnPlayOneShotEvent.Invoke(wrongCollisionAudioClip);
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

        RemoverTriggerColliders();
        OnGrabbableEvent.Invoke();
        OnPlayOneShotEvent.Invoke(grabAudioClip);
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

        OnPlayLoopEvent.Invoke(axeAudioClip);
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

        returningJourneyLength = Vector3.Distance(returningStartPosition, handTransform.position);

        if (returningJourneyLength < throwThreshold)
        {
            isThrowToClose = true;
        }
        else
        {
            isThrowToClose = false;
            if (transform.forward.z < 0)
            {
                returningMiddlePosition = returningStartPosition + (handTransform.position - returningStartPosition) / 2 + (-Vector3.forward * returningArcZ);
            }
            else
            {
                returningMiddlePosition = returningStartPosition + (handTransform.position - returningStartPosition) / 2 + (Vector3.forward * returningArcZ);
            }
        }

        TriggerColliders();
        OnPlayOneShotEvent.Invoke(returningAudioClip);
        OnReturningEvent.Invoke();
        axeState = AxeState.Returning;
    }

    private void OnAxeReturning()
    {
        float distCovered = (Time.time - returningStartTime) * returningJourneyLength;
        float fracJourney = distCovered / returningJourneyLength;

        if (isThrowToClose)
        {
            transform.position = Vector3.Lerp(returningStartPosition, handTransform.position, fracJourney * 2);
        }
        else
        {
            Vector3 point1 = Vector3.Lerp(returningStartPosition, returningMiddlePosition, fracJourney);
            Vector3 point2 = Vector3.Lerp(returningMiddlePosition, handTransform.position, fracJourney);

            transform.position = Vector3.Lerp(point1, point2, fracJourney);
        }

        axeMeshTransform.Rotate(0, 0, rotationSpeedWhenReturning * Time.deltaTime, Space.Self);
    }

    private void TriggerColliders()
    {
        foreach (Collider collider in colliderTrigabbles)
        {
            collider.isTrigger = true;
        }
    }

    private void RemoverTriggerColliders()
    {
        foreach (Collider collider in colliderTrigabbles)
        {
            collider.isTrigger = false;
        }
    }
}
