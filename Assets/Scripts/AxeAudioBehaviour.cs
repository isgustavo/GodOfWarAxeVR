using UnityEngine;

public class AxeAudioBehaviour : MonoBehaviour {

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        Debug.Log("Play Audio");
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void PlayLoop(AudioClip audioClip)
    {
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
