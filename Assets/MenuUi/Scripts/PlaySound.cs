using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private bool _playOnEnable = false;

    private void OnEnable()
    {
        if(_playOnEnable && _audioSource.clip != null)_audioSource.Play();
    }

    public void Play()
    {
        if (_audioSource.clip != null) _audioSource.Play();
    }
}
