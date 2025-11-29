using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] private MusicHandler.MusicSwitchType _musicSwitchType;
    [SerializeField] AudioCategoryType _musicCategory;
    [SerializeField] string _musicName;
    [Header("If in a jukebox area")]
    [SerializeField] private bool _useJukeboxSection = false;
    [SerializeField] private TMPro.TextMeshProUGUI _musicNameText;
    [SerializeField] private SettingsCarrier.JukeboxPlayArea _currentJukeboxPlayArea;

    //private string returnString = "";

    void OnEnable() { StartCoroutine(WaitForRequierdClasses()); }

    private IEnumerator WaitForRequierdClasses()
    {
        yield return new WaitUntil(() => (AudioManager.Instance != null && MusicReference.Instance != null));

        if (_useJukeboxSection)
            StartTrackControlExpanded();
        else
            StartTrackControlStandard();
    }

    private string StartTrackControlExpanded()
    {
        AudioManager.Instance?.SetCurrentAreaCategoryName(_musicCategory.ToString());

        bool allowedToPlay = SettingsCarrier.Instance.CanPlayJukeboxInArea(_currentJukeboxPlayArea);

        if (JukeboxManager.Instance != null && allowedToPlay)
        {
            string result = JukeboxManager.Instance.TryPlayTrack();

            if (!string.IsNullOrEmpty(result))
            {
                if (_musicNameText != null)
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

        return AudioManager.Instance.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType);
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

        return AudioManager.Instance.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType);
    }

    /// <summary>
    /// Plays the given track or the jukebox if jukebox playback is allowed from settings.
    /// </summary>
    /// <param name="musicCategory"></param>
    /// <param name="musicName"></param>
    /// <param name="musicSwitchType"></param>
    /// <param name="currentJukeboxPlayArea"></param>
    /// <returns>Name of the track that started playing or null if it failed to start.</returns>
    public static string Play(AudioCategoryType musicCategory, string musicName, MusicHandler.MusicSwitchType musicSwitchType, SettingsCarrier.JukeboxPlayArea currentJukeboxPlayArea)
    {
        PlayMusic playMusic = new PlayMusic();

        playMusic._musicCategory = musicCategory;
        playMusic._musicName = musicName;
        playMusic._musicSwitchType = musicSwitchType;
        playMusic._currentJukeboxPlayArea = currentJukeboxPlayArea;

        return new string(playMusic.StartTrackControlExpanded());
    }

    /// <summary>
    /// Plays the given track.
    /// </summary>
    /// <param name="musicCategory"></param>
    /// <param name="musicName"></param>
    /// <param name="musicSwitchType"></param>
    /// <param name="currentJukeboxPlayArea"></param>
    /// <returns>Name of the track that started playing or null if it failed to start.</returns>
    public static string Play(AudioCategoryType musicCategory, string musicName, MusicHandler.MusicSwitchType musicSwitchType)
    {
        PlayMusic playMusic = new PlayMusic();

        playMusic._musicCategory = musicCategory;
        playMusic._musicName = musicName;
        playMusic._musicSwitchType = musicSwitchType;

        return new string(playMusic.StartTrackControlStandard());
    }
}
