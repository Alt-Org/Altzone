using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolume : MonoBehaviour
{
    [SerializeField] SettingsCarrier.SoundType _soundType;

    private AudioSource m_AudioSource;
    private void Start()
    {
        m_AudioSource = gameObject.GetComponent<AudioSource>();

        m_AudioSource.volume = SettingsCarrier.Instance.SentVolume(_soundType);
    }
}
