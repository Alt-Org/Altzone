using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the visual queue item.
/// </summary>
public class JukeboxTrackQueueHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;
    [SerializeField] private Button _likeOptionButton;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private TextAutoScroll _textAutoScroll;
    [SerializeField] private FavoriteButtonHandler _favoriteButtonHandler;
    [Space]
    [SerializeField] private float _buttonPressCancelTime = 0.25f;

    private bool _buttonInputCanceled = false;

    private Coroutine _buttonCancelCoroutine;

    private string _id; // Id that is used with JukeboxManager.
    public string Id { get { return _id; } }

    //Index of it self in the JukeboxManager's TrackQueue list.
    private int _linearIndex = -1;
    public int LinearIndex { get { return _linearIndex; } }

    private int _chunkIndex = 0;
    public int ChunkIndex { get { return _chunkIndex; } }

    private int _poolIndex = 0;
    public int PoolIndex { get { return _poolIndex; } }

    private bool _userOwned = false;
    public bool UserOwned { get { return _userOwned; } }

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack { get { return _musicTrack; } }

    public delegate void DeleteEvent(int chunkIndex, int poolIndex, int linearIndex);
    public event DeleteEvent OnDeleteEvent;

    private void Start()
    {
        _deleteButton.onClick.AddListener(() => Delete());
    }

    /// <summary>
    /// Use when creating the gameobject that has this class. (Execute only once!)
    /// </summary>
    public void Setup(int chunkIndex, int poolIndex)
    {
        _chunkIndex = chunkIndex;
        _poolIndex = poolIndex;
        SetVisibility(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_buttonCancelCoroutine != null)
        {
            StopCoroutine(CancelButtonPress());
            _buttonCancelCoroutine = null;
        }

        _buttonCancelCoroutine = StartCoroutine(CancelButtonPress());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_buttonCancelCoroutine != null)
        {
            StopCoroutine(CancelButtonPress());
            _buttonCancelCoroutine = null;
        }
    }

    private IEnumerator CancelButtonPress()
    {
        float timer = 0f;

        while (timer < _buttonPressCancelTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _buttonInputCanceled = true;
    }

    public bool InUse() { return !string.IsNullOrEmpty(_id); }

    public void SetTrack(string id, MusicTrack musicTrack, int linearIndex, bool userOwned, JukeboxManager.MusicTrackFavoriteType likeType)
    {
        _id = id;
        _musicTrack = musicTrack;
        _linearIndex = linearIndex;

        if (musicTrack != null)
            _trackNameText.text = musicTrack.Name;
        else
            _trackNameText.text = "";

        if (GetVisibility())
            _textAutoScroll.SetContent(musicTrack.Name);
        else
            _textAutoScroll.DisableCoroutines();

        _userOwned = userOwned;
        _deleteButton.gameObject.SetActive(userOwned);
        _favoriteButtonHandler.Setup(likeType, musicTrack.Id);
    }

    public void SetLinearIndex(int index) { _linearIndex = index; }

    public void Clear() { _id = ""; _musicTrack = null; _trackNameText.text = ""; _linearIndex = -1; SetVisibility(false); }

    public void SetVisibility(bool visible) { gameObject.SetActive(visible); }

    public bool GetVisibility() { return gameObject.activeSelf; }

    private void Delete() { if (!_buttonInputCanceled) OnDeleteEvent.Invoke(_chunkIndex, _poolIndex, _linearIndex); }
}
