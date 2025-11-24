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

    void Start() { StartCoroutine(StartTrackControl()); }

    private IEnumerator StartTrackControl()
    {
        yield return new WaitUntil(() => (AudioManager.Instance != null && MusicReference.Instance != null));

        MusicTrack startMusicTrack = MusicReference.Instance.GetTrack(_musicCategory, _musicName);

        if (startMusicTrack == null)
        {
            Debug.LogError("Start track is null!");
            yield break;
        }

        AudioManager.Instance.PlayMusic(_musicCategory, startMusicTrack, _musicSwitchType);
    }
}
