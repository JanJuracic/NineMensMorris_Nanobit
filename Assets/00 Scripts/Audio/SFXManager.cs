using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSourcePrefab;

    public static SFXManager Instance;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        Instance = null;
    }

    public static void Play(AudioClip clip, Transform spawnTr, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(Instance.audioSourcePrefab, spawnTr.transform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
}
