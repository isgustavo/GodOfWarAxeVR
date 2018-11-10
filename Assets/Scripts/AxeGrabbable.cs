using System;
using UnityEngine;

public class AxeGrabbable : OVRGrabbable
{
    private float angularVelocityThreshold = 350f;

    public Action OnAxeGrabbableAction = delegate { };
    public Action OnAxeDroppedAction = delegate { };
    public Action<Vector3, Vector3> OnAxeTrownAction = delegate { };

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        //base.GrabBegin(hand, grabPoint);
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;

        OnAxeGrabbableAction.Invoke();
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        //base.GrabEnd(linearVelocity, angularVelocity);
        if (Mathf.Abs((int)angularVelocity.x) < angularVelocityThreshold)
        {
            OnAxeDroppedAction.Invoke();
        }
        else
        {
            OnAxeTrownAction.Invoke(linearVelocity, angularVelocity);
        }

        m_grabbedBy = null;
        m_grabbedCollider = null;
    }
}
