using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHandler : MonoBehaviour
{
    private AudioSource _audioSource;

    public delegate void PlaybackFinished(int chunk, int channel);
    public event PlaybackFinished OnPlaybackFinished;

    private int _chunkIndex = 0;
    private int _channelIndex = 0;
    private Coroutine _playbackCoroutine;

    public void SetChunkIndex(int chunk, int channel) { _chunkIndex = chunk; _channelIndex = channel; }

    void Start() { if (!_audioSource) GetAudioSource(); }

    public void SetVolume(float volume)
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.volume = volume;
    }

    public void SetAudioClip(AudioClip audioClip)
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.clip = audioClip;
    }

    public void SetPlayAudioClip(AudioClip audioClip, bool loop, float pitch)
    {
        SetAudioClip(audioClip);
        SetLoop(loop);
        SetPitch(pitch);
        Play();
    }

    public void SetLoop(bool value)
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.loop = value;
    }

    public bool IsInUse()
    {
        if (!_audioSource) GetAudioSource();

        return _audioSource.clip != null;
    }

    public void Clear()
    {
        Stop();
        _audioSource.clip = null;
        _audioSource.pitch = 1f;
    }

    public void Play()
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.Play();

        if (!_audioSource.loop) _playbackCoroutine = StartCoroutine(WaitForPlaybackFinish());
    }

    public void Stop()
    {
        if (!_audioSource) GetAudioSource();

        StopCoroutine(_playbackCoroutine);
        _audioSource.Stop();
    }

    public void Continue()
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.UnPause();
        StartCoroutine(WaitForPlaybackFinish());
    }

    public void SetPitch(float pitch)
    {
        if (!_audioSource) GetAudioSource();

        _audioSource.pitch = pitch;
    }

    private IEnumerator WaitForPlaybackFinish()
    {
        while (_audioSource.isPlaying)
            yield return null;

        _audioSource.clip = null;
        OnPlaybackFinished?.Invoke(_chunkIndex, _channelIndex);
    }

    private void GetAudioSource() { _audioSource = GetComponent<AudioSource>(); }
}
