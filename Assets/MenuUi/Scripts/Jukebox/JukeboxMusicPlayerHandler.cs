using System.Collections;
using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxMusicPlayerHandler : MonoBehaviour
{
    [Header("MusicPlayer")]
    [SerializeField] private Slider _trackPlayTimeSlider;
    [SerializeField] private SliderRubberband _sliderRubberband;
    [SerializeField] private float _sliderRubberbandAnimationTreshold = 0.01f;

    private bool _sliderRubberbandActive = false;
    private float _currentTrackLength = 0f;
    private Coroutine _sliderAnimationCoroutine;

    [Header("QueueList")]
    [SerializeField] private SmartVerticalObjectList _smartList;
    [Header("List Windows")]
    [SerializeField] private GameObject _tracksListObject;
    [SerializeField] private GameObject _queueListObject;
    [Space]
    [SerializeField] private Toggle _tracksToggle;
    [SerializeField] private Toggle _queueToggle;

    private enum ListPageType
    {
        Tracks,
        Queue
    }

    private ListPageType _currentListPage;

    void Awake()
    {
        _tracksToggle.onValueChanged.AddListener((value) => { if (value) ChangeListPage(ListPageType.Tracks); });
        _queueToggle.onValueChanged.AddListener((value) => { if (value) ChangeListPage(ListPageType.Queue); });
    }

    private void Start()
    {
        _smartList.OnNewDataRequested += UpdateTrackHandler;
        _smartList.OnLateDataRequest += UpdateVisualQueueList;
        StartCoroutine(Setup());
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateMusicElapsedTime;
        JukeboxManager.Instance.OnQueueChange += UpdateVisualQueueList;
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnSetVisibleElapsedTime -= UpdateMusicElapsedTime;
        JukeboxManager.Instance.OnQueueChange -= UpdateVisualQueueList;
    }

    private IEnumerator Setup()
    {
        yield return new WaitUntil(() => JukeboxManager.Instance.PlaylistReady);

        UpdateVisualQueueList();
    }

    private void ChangeListPage(ListPageType type)
    {
        switch (_currentListPage)
        {
            case ListPageType.Tracks: _tracksListObject.SetActive(false); break;
            case ListPageType.Queue: _queueListObject.SetActive(false); break;
        }

        switch (type)
        {
            case ListPageType.Tracks: _tracksListObject.SetActive(true); break;
            case ListPageType.Queue: _queueListObject.SetActive(true); break;
        }

        _currentListPage = type;
    }

    private void UpdateVisualQueueList() { _smartList.Setup<TrackQueueData>(JukeboxManager.Instance.TrackQueue); }

    private void UpdateTrackHandler(int index) { _smartList.UpdateContent(index, JukeboxManager.Instance.TrackQueue[index]); }

    private void UpdateMusicElapsedTime(float musicTrackLength, float elapsedTime, JukeboxManager.PreviewLocationType type, bool playAnimations = true)
    {
        if (_sliderRubberbandActive && _currentTrackLength != musicTrackLength)
        {
            if (_sliderAnimationCoroutine != null)
            {
                StopCoroutine(_sliderAnimationCoroutine);
                _sliderAnimationCoroutine = null;
            }
            _sliderRubberbandActive = false;
        }

        if (!_sliderRubberbandActive && Mathf.Abs(_trackPlayTimeSlider.value - (elapsedTime / musicTrackLength)) > _sliderRubberbandAnimationTreshold && playAnimations)
        {
            _currentTrackLength = musicTrackLength;
            _sliderRubberbandActive = true;

            _sliderAnimationCoroutine = StartCoroutine(_sliderRubberband.StartRubberband(elapsedTime,
                musicTrackLength, (data) => _sliderRubberbandActive = !data));
        }

        if (!_sliderRubberbandActive) _trackPlayTimeSlider.value = elapsedTime / musicTrackLength;
    }
}
