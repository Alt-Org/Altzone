using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] private bool _startOnEnable = true;
    [SerializeField] private MusicHandler.MusicSwitchType _musicSwitchType;
    [SerializeField] AudioCategoryType _musicCategory;
    [SerializeField] string _musicName;
    [Header("If in a jukebox area")]
    [SerializeField] private bool _useJukeboxSection = false;
    [SerializeField] private TMPro.TextMeshProUGUI _musicNameText;
    [SerializeField] private SettingsCarrier.JukeboxPlayArea _currentJukeboxPlayArea;

    void OnEnable() { StartCoroutine(WaitForRequiredClasses()); }

    private IEnumerator WaitForRequiredClasses()
    {
        if (!_startOnEnable) yield break;

        yield return new WaitUntil(() => (AudioManager.Instance && MusicReference.Instance));

        if (_useJukeboxSection)
            StartTrackControlExpanded(false);
        else
            StartTrackControlStandard();
    }

    private string StartTrackControlExpanded(bool playAnimations = true)
    {
        AudioManager.Instance?.SetCurrentAreaCategoryName(_musicCategory.ToString());

        bool allowedToPlay = SettingsCarrier.Instance.CanPlayJukeboxInArea(_currentJukeboxPlayArea);

        if (JukeboxManager.Instance && allowedToPlay)
        {
            string result = JukeboxManager.Instance.TryPlayTrack(playAnimations);

            if (!string.IsNullOrEmpty(result))
            {
                if (_musicNameText)
                    _musicNameText.text = result;
                else
                    return result;

                return "";
            }
        }
        else
            Debug.LogWarning("Could not try to start jukebox playback.");

        MusicTrack startMusicTrack = MusicReference.Instance.GetTrack(_musicCategory, _musicName);

        if (startMusicTrack == null)
        {
            Debug.LogError("Start track is null!");
            return "";
        }

        return (AudioManager.Instance ? AudioManager.Instance.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType) : "");
    }

    private string StartTrackControlStandard()
    {
        AudioManager.Instance?.SetCurrentAreaCategoryName(_musicCategory.ToString());

        MusicTrack startMusicTrack = MusicReference.Instance.GetTrack(_musicCategory, _musicName);

        if (startMusicTrack == null)
        {
            Debug.LogError("Start track is null!");
            return "";
        }

        return (AudioManager.Instance ? AudioManager.Instance.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType) : "");
    }

    public void Play() { AudioManager.Instance?.PlayMusic(_musicCategory, _musicName, _musicSwitchType); }

    // /// <summary>
    // /// Plays the given track or the jukebox if jukebox playback is allowed from settings.
    // /// </summary>
    // /// <param name="musicCategory"></param>
    // /// <param name="musicName"></param>
    // /// <param name="musicSwitchType"></param>
    // /// <param name="currentJukeboxPlayArea"></param>
    // /// <returns>Name of the track that started playing or null if it failed to start.</returns>
    // public static string Play(AudioCategoryType musicCategory, string musicName, MusicHandler.MusicSwitchType musicSwitchType, SettingsCarrier.JukeboxPlayArea currentJukeboxPlayArea)
    // {
    //     PlayMusic playMusic = new PlayMusic();
    //
    //     playMusic._musicCategory = musicCategory;
    //     playMusic._musicName = musicName;
    //     playMusic._musicSwitchType = musicSwitchType;
    //     playMusic._currentJukeboxPlayArea = currentJukeboxPlayArea;
    //
    //     return new string(playMusic.StartTrackControlExpanded());
    // }
    //
    // /// <summary>
    // /// Plays the given track.
    // /// </summary>
    // /// <param name="musicCategory"></param>
    // /// <param name="musicName"></param>
    // /// <param name="musicSwitchType"></param>
    // /// <param name="currentJukeboxPlayArea"></param>
    // /// <returns>Name of the track that started playing or null if it failed to start.</returns>
    // public static string Play(AudioCategoryType musicCategory, string musicName, MusicHandler.MusicSwitchType musicSwitchType)
    // {
    //     PlayMusic playMusic = new PlayMusic();
    //
    //     playMusic._musicCategory = musicCategory;
    //     playMusic._musicName = musicName;
    //     playMusic._musicSwitchType = musicSwitchType;
    //
    //     return new string(playMusic.StartTrackControlStandard());
    // }
}
