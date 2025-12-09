using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxPreviewHandler : MonoBehaviour
{
    [SerializeField] private SliderRubberband _sliderRubberband;
    [SerializeField] private Button _playPreviewButton;

    [SerializeField] private CanvasGroup _sliderCanvasGroup;
    [SerializeField] private CanvasGroup _buttonCanvasGroup;

    [SerializeField] private float _indicatorSwitchDuration = 0.5f;
    [SerializeField] private AnimationCurve _indicatorSwitchAnimationCurve;

    [SerializeField] private Slider _previewSlider;

    private bool _sliderRubberbandActive = false;
    private MusicTrack _musicTrack;

    private Coroutine _switchIndicatorCoroutine;
    private Coroutine _sliderRubberbandCoroutine;

    public delegate void StartPreview(MusicTrack musicTrack, float previewDuration = -1f);
    public event StartPreview OnStartPreview;

    public enum IndicatorType
    {
        PlayButton,
        ProgressSlider
    }

    private void Awake()
    {
        _playPreviewButton.onClick.AddListener(() => StartCoroutine(Preview()));
    }

    private void OnEnable()
    {
        JukeboxManager.Instance.OnSetVisibleElapsedTime += UpdateSlider;
        JukeboxManager.Instance.OnPreviewEnd += StartStoppingMusicPreview;
        
        _sliderCanvasGroup.blocksRaycasts = false;
        _buttonCanvasGroup.blocksRaycasts = true;
        _sliderCanvasGroup.alpha = 0f;
        _buttonCanvasGroup.alpha = 1f;
        _previewSlider.value = 1f;
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnSetVisibleElapsedTime -= UpdateSlider;
        JukeboxManager.Instance.OnPreviewEnd -= StartStoppingMusicPreview;

        ForceStopPreview();
    }

    public void SetMusicTrack(MusicTrack musicTrack) { _musicTrack = musicTrack; }

    private void StopLocalCoroutines()
    {
        if (_switchIndicatorCoroutine != null)
        {
            StopCoroutine(_switchIndicatorCoroutine);
            _switchIndicatorCoroutine = null;
        }

        if (_sliderRubberbandCoroutine != null)
        {
            StopCoroutine(_sliderRubberbandCoroutine);
            _sliderRubberbandCoroutine = null;
        }
    }

    private IEnumerator Preview()
    {
        bool? rubberbandDone = null;

        if (_musicTrack == null)
        {
            Debug.LogError("MusicTrack is null!");
            yield break;
        }

        _sliderRubberbandActive = true;
        _buttonCanvasGroup.blocksRaycasts = false;

        OnStartPreview.Invoke(_musicTrack, _musicTrack.Music.length);

        StopLocalCoroutines();

        _switchIndicatorCoroutine = StartCoroutine(SwitchIndicator(IndicatorType.ProgressSlider));
        _sliderRubberbandCoroutine = StartCoroutine(_sliderRubberband.StartRubberband(0f, _musicTrack.Music.length, (data) => rubberbandDone = data));

        yield return new WaitUntil(() => rubberbandDone != null);

        _sliderCanvasGroup.blocksRaycasts = true;
        _sliderRubberbandActive = false;
    }

    private IEnumerator SwitchIndicator(IndicatorType visibleType)
    {
        float timer = 0f;

        CanvasGroup targetOn = (visibleType == IndicatorType.PlayButton ? _buttonCanvasGroup : _sliderCanvasGroup);
        CanvasGroup targetOff = (visibleType == IndicatorType.PlayButton ? _sliderCanvasGroup : _buttonCanvasGroup); ;

        while (timer < _indicatorSwitchDuration)
        {
            yield return null;
            timer += Time.deltaTime;

            float progress = _indicatorSwitchAnimationCurve.Evaluate(timer / _indicatorSwitchDuration);

            targetOn.alpha = Mathf.Lerp(0f, 1f, progress);
            targetOff.alpha = Mathf.Lerp(1f, 0f, progress);
        }
    }

    private void UpdateSlider(float musicTrackLength, float elapsedTime)
    {
        if (!JukeboxManager.Instance.TrackPreviewActive) return;

        if (!_sliderRubberbandActive) _previewSlider.value = elapsedTime / musicTrackLength;
    }

    private void StartStoppingMusicPreview()
    {
        if (isActiveAndEnabled)
            StartCoroutine(PreviewStop());
        else
            ForceStopPreview();
    }

    private IEnumerator PreviewStop()
    {
        bool? rubberbandDone = null;
        _sliderRubberbandActive = true;
        _sliderCanvasGroup.blocksRaycasts = false;

        StopLocalCoroutines();

        _switchIndicatorCoroutine = StartCoroutine(SwitchIndicator(IndicatorType.PlayButton));
        _sliderRubberbandCoroutine = StartCoroutine(_sliderRubberband.StartRubberband(1f, 1f, (data) => rubberbandDone = data));

        yield return new WaitUntil(() => rubberbandDone != null);

        _buttonCanvasGroup.blocksRaycasts = true;
        _sliderRubberbandActive = false;
    }

    private void ForceStopPreview()
    {
        StopLocalCoroutines();
    }
}
