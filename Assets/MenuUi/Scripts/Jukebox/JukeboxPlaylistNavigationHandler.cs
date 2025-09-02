using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxPlaylistNavigationHandler : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private TMP_InputField _searchField;
    [SerializeField] private Button _tracksFilterButton;
    [Space]
    [SerializeField] private Transform _tracksListContent;
    [SerializeField] private GameObject _jukeboxButtonPrefab;
    [Space]
    [SerializeField] private int _trackChunkSize = 8;
    private List<Chunk<JukeboxTrackButtonHandler>> _buttonHandlerChunks = new List<Chunk<JukeboxTrackButtonHandler>>(); //Visible
    private int _buttonHandlerChunkPointer = 0;
    private int _buttonHandlerPoolPointer = -1;

    private void Start()
    {
        CreateButtonHandlersChunk();

        List<MusicTrack> musicTracks = AudioManager.Instance.GetMusicList("Jukebox");

        foreach (MusicTrack track in musicTracks) GetFreeJukeboxTrackButtonHandler().SetTrack(track);
    }

    private void CreateButtonHandlersChunk()
    {
        List<JukeboxTrackButtonHandler> jukeboxButtonHandlers = new List<JukeboxTrackButtonHandler>(_trackChunkSize);

        for (int i = 0; i < _trackChunkSize; i++)
        {
            GameObject jukeboxTrackButton = Instantiate(_jukeboxButtonPrefab, _tracksListContent);
            JukeboxTrackButtonHandler buttonHandler = jukeboxTrackButton.GetComponent<JukeboxTrackButtonHandler>();
            buttonHandler.OnTrackPressed += JukeboxManager.Instance.QueueTrack;
            buttonHandler.Clear();
            jukeboxButtonHandlers.Add(buttonHandler);
        }

        Chunk<JukeboxTrackButtonHandler> tracksChunk = new Chunk<JukeboxTrackButtonHandler>();
        tracksChunk.Pool = jukeboxButtonHandlers;
        tracksChunk.AmountInUse = 0;

        _buttonHandlerChunks.Add(tracksChunk);
    }

    private JukeboxTrackButtonHandler GetFreeJukeboxTrackButtonHandler()
    {
        _buttonHandlerPoolPointer++;

        if (_buttonHandlerPoolPointer >= _trackChunkSize)
        {
            CreateButtonHandlersChunk();
            _buttonHandlerChunkPointer++;
            _buttonHandlerPoolPointer = 0;
        }

        _buttonHandlerChunks[_buttonHandlerChunkPointer].AmountInUse++;

        return _buttonHandlerChunks[_buttonHandlerChunkPointer].Pool[_buttonHandlerPoolPointer];
    }
}
