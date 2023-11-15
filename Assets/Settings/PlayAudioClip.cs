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
            SettingsCarrier.Instance.PlaySound(_audioSource.clip, SettingsCarrier.Instance.SentVolume(GetComponent<SetVolume>()._soundType));
    }
}
