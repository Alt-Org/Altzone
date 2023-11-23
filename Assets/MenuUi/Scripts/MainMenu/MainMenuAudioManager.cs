using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAudioManager : MonoBehaviour
{
    public static MainMenuAudioManager Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public void PlaySound(AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }
}
