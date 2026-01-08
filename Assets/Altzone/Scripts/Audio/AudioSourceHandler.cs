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

    void Start() { _audioSource = GetComponent<AudioSource>(); }

    public void SetVolume(float volume) { _audioSource.volume = volume; }

    public void SetAudioClip(AudioClip audio) { _audioSource.clip = audio; }

    public void SetPlayAudioClip(AudioClip audio, bool loop, float pitch) { SetAudioClip(audio); SetLoop(loop); SetPitch(pitch); Play(); }

    public void SetLoop(bool value) { _audioSource.loop = value; }

    public bool IsInUse() { return _audioSource.clip != null; }

    public void Clear() { Stop(); _audioSource.clip = null; _audioSource.pitch = 1f; }

    public void Play()
    {
        _audioSource.Play();

        if (!_audioSource.loop) _playbackCoroutine = StartCoroutine(WaitForPlaybackFinish());
    }

    public void Stop() { StopCoroutine(_playbackCoroutine); _audioSource.Stop(); }

    public void Continue() { _audioSource.UnPause(); StartCoroutine(WaitForPlaybackFinish()); }

    public void SetPitch(float pitch) { _audioSource.pitch = pitch; }

    private IEnumerator WaitForPlaybackFinish()
    {
        while (_audioSource.isPlaying)
            yield return null;

        _audioSource.clip = null;
        OnPlaybackFinished.Invoke(_chunkIndex, _channelIndex);
    }
}
