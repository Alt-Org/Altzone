using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SettingsCarrier;

public class PlayAudioClip : MonoBehaviour
{
    private AudioSource _audioSource;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio()
    {
        if (_audioSource != null)
            MainMenuAudioManager.Instance.PlaySound(_audioSource.clip, GetComponent<AudioSource>().volume);
    }
}
