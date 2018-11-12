using System;
using System.Collections;
using UnityEngine;

public class PowerfulHandBehaviour : MonoBehaviour
{
    [SerializeField]
    private OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField]
    private OVRInput.RawAxis1D trigger = OVRInput.RawAxis1D.RHandTrigger;

    public Action OnPowerHandEvent = delegate { };

    private void OnEnable()
    {
        StartCoroutine(OnListenerHandCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    protected IEnumerator OnListenerHandCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(.5f);

            if (OVRInput.IsControllerConnected(controller)
                && OVRInput.Get(trigger, controller) == 0
                && transform.localPosition.y * 100 > 0)
            {
                OnPowerHandEvent.Invoke();
            }
        }

    }
}
