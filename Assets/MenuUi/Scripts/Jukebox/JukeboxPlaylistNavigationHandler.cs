using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;

public class JukeboxPlaylistNavigationHandler : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private TMP_InputField _searchField;
    [Space]
    [SerializeField] private SmartVerticalObjectList _smartList;

    private List<Chunk<bool>> _hiddenTrackHandlers = new List<Chunk<bool>>();
    private int _previousSearchLength = 0;

    private List<PersonalizedMusicTrack> _personalizedMusicTracks = new();

    private void Start()
    {
        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox"); //TODO: Replace with what tracks the clan actually owns when possible.

        FillSelectionButtonList(musicTracks);
        _smartList.OnNewDataRequested += UpdateButtonHandlerData;

        #region Filters
        _searchField.onValueChanged.AddListener((value) => SearchFieldChange(value));
        #endregion
    }

    private void FillSelectionButtonList(List<MusicTrack> musicTracks)
    {
        _personalizedMusicTracks.Clear();

        foreach (MusicTrack musicTrack in musicTracks)
            _personalizedMusicTracks.Add(new PersonalizedMusicTrack(musicTrack, JukeboxManager.Instance.GetTrackFavoriteType(musicTrack.Id)));

        _smartList.Setup<PersonalizedMusicTrack>(_personalizedMusicTracks);
    }

    private void UpdateButtonHandlerData(int targetIndex)
    {
        _smartList.UpdateContent<PersonalizedMusicTrack>(targetIndex, _personalizedMusicTracks[targetIndex]);
    }

    #region Filtering
    private void SearchFieldChange(string value) //TODO: Useless repetition?
    {
        List<MusicTrack> musicTracks;

        if (string.IsNullOrEmpty(value)) //Set all track button handlers visible that have a music track.
        {
            musicTracks = AudioManager.Instance.GetMusicList("Jukebox"); //TODO: Replace with what tracks the clan actually owns when possible.

            FillSelectionButtonList(musicTracks);

            return;
        }

        musicTracks = AudioManager.Instance.GetMusicList("Jukebox").
            FindAll((data) => data.Name.Contains(value, System.StringComparison.CurrentCultureIgnoreCase)); //TODO: Replace with what tracks the clan actually owns when possible.

        FillSelectionButtonList(musicTracks);
    }
    #endregion
}
