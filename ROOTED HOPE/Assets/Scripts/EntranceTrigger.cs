using UnityEngine;
using System.Collections;

public class EntranceTrigger : MonoBehaviour
{
    public AudioSource waterAudio;
    public float fadeInTime = 3f;
    public float fadeOutTime = 4f;
    private bool hasEntered = false;

    void Start()
    {
        waterAudio.volume = 0f;
        waterAudio.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasEntered)
        {
            hasEntered = true;
            StartCoroutine(FadeVolume(waterAudio, 0f, 0.85f, fadeInTime));
        }
    }

    void OnTriggerExit(Collider other)
    {
        StartCoroutine(FadeVolume(waterAudio, waterAudio.volume, 0f, fadeOutTime));
        hasEntered = false;
    }

    IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        source.volume = to;
    }
}