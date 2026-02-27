using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

public class LoopingTrackWithStart : MonoBehaviour
{
    [SerializeField] private MusicHandler.MusicSwitchType _startMusicSwitchType;
    [SerializeField] AudioCategoryType _startMusicCategory;
    [SerializeField] string _startMusicName;
    [Space]
    [SerializeField] private MusicHandler.MusicSwitchType _loopMusicSwitchType;
    [SerializeField] AudioCategoryType _loopMusicCategory;
    [SerializeField] string _loopMusicName;

    void Start() { StartCoroutine(StartTrackControl()); }

    private IEnumerator StartTrackControl()
    {
        yield return new WaitUntil(() => (AudioManager.Instance != null && MusicReference.Instance != null));

        float introTimer = 0f;
        MusicTrack startMusicTrack = MusicReference.Instance.GetTrack(_startMusicCategory, _startMusicName);

        if (startMusicTrack == null)
        {
            Debug.LogError("Start track is null!");
            yield break;
        }

        AudioManager.Instance.PlayMusic(_startMusicCategory, startMusicTrack, _startMusicSwitchType);

        while (introTimer < startMusicTrack.Music.length)
        {
            yield return null;
            introTimer += Time.deltaTime;
        }

        MusicTrack loopMusicTrack = MusicReference.Instance.GetTrack(_loopMusicCategory, _loopMusicName);

        if (loopMusicTrack == null)
        {
            Debug.LogError("Loop track is null!");
            yield break;
        }

        AudioManager.Instance.PlayMusic(_loopMusicCategory, loopMusicTrack, _loopMusicSwitchType);
    }
}
