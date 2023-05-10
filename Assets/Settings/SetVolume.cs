using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolume : MonoBehaviour
{
    [SerializeField] SettingsCarrier.SoundType _soundType;
    private void Start()
    {
        VolumeSet();
    }

    public void VolumeSet()
    {
        // Gets the master volume and the volume for the type set in _soundType from SettingsCarrier
        gameObject.GetComponent<AudioSource>().volume = SettingsCarrier.Instance.SentVolume(_soundType);
    }
}
