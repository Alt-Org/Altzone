using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField] private bool _startOnEnable = true;
    [SerializeField] private MusicHandler.MusicSwitchType _musicSwitchType;
    [SerializeField] AudioCategoryType _musicCategory;
    [SerializeField] string _musicName;

    void OnEnable() { StartCoroutine(WaitForRequiredClasses()); }

    private IEnumerator WaitForRequiredClasses()
    {
        if (!_startOnEnable) yield break;

        yield return new WaitUntil(() => (AudioManager.Instance && MusicReference.Instance));

        StartTrackControlStandard();
    }

    private void StartTrackControlStandard()
    {
        MusicTrack startMusicTrack = MusicReference.Instance.GetTrack(_musicCategory, _musicName);

        if (startMusicTrack == null)
        {
            Debug.LogError("Start track is null!");
            return;
        }

        if (string.IsNullOrEmpty(AudioManager.Instance?.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType)))
            JukeboxManager.Instance?.TryPlayTrack();
    }

    public void Play() { AudioManager.Instance?.PlayMusic(_musicCategory, _musicName, _musicSwitchType); }
}
