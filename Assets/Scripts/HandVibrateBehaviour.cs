using System;
using System.Collections;
using UnityEngine;

public class HandVibrateBehaviour : MonoBehaviour {

    [SerializeField]
    private OVRInput.Controller controller = OVRInput.Controller.RTouch;
    [SerializeField]
    private OVRInput.RawAxis1D trigger = OVRInput.RawAxis1D.RHandTrigger;

    private OVRHapticsClip clip;
    private OVRHaptics.OVRHapticsChannel channel = OVRHaptics.RightChannel;

    private void OnEnable()
    {
        InitializeOVRHaptics();

        if (controller == OVRInput.Controller.LTouch)
            channel = OVRHaptics.LeftChannel;
    }

    private void InitializeOVRHaptics()
    {

        int cnt = 50;
        clip = new OVRHapticsClip(cnt);
        for (int i = 0; i < cnt; i++)
        {
            clip.Samples[i] = i % 2 == 0 ? (byte)0 : (byte)100;
        }

        clip = new OVRHapticsClip(clip.Samples, clip.Samples.Length);
    }

    public void Vibrate()
    {
        StopAllCoroutines();
        channel.Preempt(clip);
    }

    public void VibrateNonStop()
    {
        StartCoroutine(VibrateCoroutine());
    }

    private IEnumerator VibrateCoroutine()
    {
        while (true)
        {
            channel.Queue(clip);
            yield return new WaitForEndOfFrame();
        }
    }
}
