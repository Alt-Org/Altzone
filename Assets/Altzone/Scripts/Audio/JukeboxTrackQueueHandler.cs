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
public class JukeboxTrackQueueHandler : SmartListItem, IBeginDragHandler, IEndDragHandler
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
    private int _linearIndex = -1; //Index of itself in the JukeboxManager's TrackQueue list.
    private bool _userOwned = false;
    private MusicTrack _musicTrack = null;

    private void Awake() { _selfRectTransform = GetComponent<RectTransform>(); }

    private void Start() { _deleteButton.onClick.AddListener(() => Delete()); }

    /// <summary>
    /// Use when creating the gameobject that has this class. (Execute only once!)
    /// </summary>
    public void Setup() { SetVisibility(false); }

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

    public override void SetData<T1>(T1 data1)
    {
        if (!CheckClassType<T1, TrackQueueData>(data1)) return;

        TrackQueueData data = data1 as TrackQueueData;

        if (data?.ServerSongData == null)
        {
            ClearData();
            return;
        }

        _id = data.ServerSongData.id;
        _musicTrack = data.MusicTrack;
        _linearIndex = data.LinearIndex;

        _trackNameText.text = _musicTrack != null ? _musicTrack.Name : "";

        if (GetVisibility() && _musicTrack != null)
            _textAutoScroll.SetContent(_musicTrack.Name);
        else
            _textAutoScroll.DisableCoroutines();

        _userOwned = data.UserOwned;
        _deleteButton.gameObject.SetActive(_userOwned);

        if (_musicTrack != null) _favoriteButtonHandler.Setup(data.FavoriteType, _musicTrack.Id);

        SetVisibility(true);
    }

    public void SetLinearIndex(int index) { _linearIndex = index; }

    public override void ClearData()
    {
        _id = "";
        _musicTrack = null;
        _trackNameText.text = "";
        _linearIndex = -1;
        SetVisibility(false);
    }

    public bool GetVisibility() { return gameObject.activeSelf; }

    private void Delete()
    {
        if (!_buttonInputCanceled && _userOwned) JukeboxManager.Instance.DeleteFromQueue(_linearIndex, true);
    }
}
