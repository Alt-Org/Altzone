using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoogingTrackWithStart : MonoBehaviour
{
    [SerializeField] private AudioClip _startTrack;
    [SerializeField] private AudioClip _loopTrack;

    [Range(0f,1f)]
    [SerializeField] private float _volume;

    private AudioSource _source;

    void Start()
    {
        _source = GetComponent<AudioSource>();
        _source.volume = _volume;
        StartCoroutine(StartTrackControl());
    }

    private IEnumerator StartTrackControl()
    {
        float timer = 0f;

        _source.clip = _startTrack;
        _source.Play();

        while (timer < _startTrack.length)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _source.clip = _loopTrack;
        _source.loop = true;
        _source.Play();
    }

    private void OnDisable()
    {
        _source.Stop();
    }
}
