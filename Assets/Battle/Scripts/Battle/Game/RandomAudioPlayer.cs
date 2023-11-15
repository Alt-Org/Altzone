using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioPlayer : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] private StartSetting _startSetting;
    [SerializeField] private LoopSetting _loopSetting;
    [SerializeField] private AudioClip[] _audioList;

    // Public Properties
    public bool IsPlaying => _audioSource.isPlaying;
    public bool Loop { get => _audioSource.loop; set => _audioSource.loop = value; }

    #region Public Methods

    /// <summary>
    /// Plays currently selected audio clip
    /// </summary>
    public void Play()
    {
        _audioSource.Play();
    }

    /// <summary>
    /// Selects random audio clip and plays it
    /// </summary>
    public void PlayRandom()
    {
        _audioSource.Stop();
        _audioSource.clip = _audioList[Random.Range(0, _audioList.Length)];
        _audioSource.Play();
    }

    /// <summary>
    /// Stops playing audio clip
    /// </summary>
    public void Stop()
    {
        _audioSource.Stop();
    }

    #endregion

    // Private Enums
    private enum StartSetting { None, Play, PlayRandom }
    private enum LoopSetting { False, True, LetAudioSourceDecide }

    // Components
    AudioSource _audioSource;

    void Start()
    {
        // get components
        _audioSource = GetComponent<AudioSource>();

        switch (_loopSetting)
        {
            case LoopSetting.False:
                _audioSource.loop = false;
                break;

            case LoopSetting.True:
                _audioSource.loop = true;
                break;

            case LoopSetting.LetAudioSourceDecide:
                break;
        }

        switch (_startSetting)
        {
            case StartSetting.None:
                break;

            case StartSetting.Play:
                Play();
                break;

            case StartSetting.PlayRandom:
                PlayRandom();
                break;
        }
    }
}
