using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxSecondaryDiskHandler : MonoBehaviour
{
    [SerializeField] private Image _mainDiskImage;
    [SerializeField] private Sprite _emptyDiskSprite;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [Header("Switch Disk Animation")]
    [SerializeField] private bool _useAnimation = false;
    [SerializeField] private float _switchDiskAnimDuration = 1f;
    [SerializeField] private AnimationCurve _switchDiskAnimCurve;
    [SerializeField] private RectTransform _mainDiskRectTransform;
    [SerializeField] private RectTransform _secondaryDiskRectTransform;
    [SerializeField] private Image _secondaryDiskImage;

    private Coroutine _diskSpinCoroutine;

    #region Animation
    private Vector2 _mainAnchorMinStart = new Vector2(1f, 0f);
    private Vector2 _mainAnchorMaxStart = new Vector2(1f, 1f);
    private Vector2 _mainAnchorMinEnd = new Vector2(0f, 0f);
    private Vector2 _mainAnchorMaxEnd = new Vector2(1f, 1f);

    private Vector2 _secondaryAnchorMinStart = new Vector2(0f, 0f);
    private Vector2 _secondaryAnchorMaxStart = new Vector2(1f, 1f);
    private Vector2 _secondaryAnchorMinEnd = new Vector2(0f, 0f);
    private Vector2 _secondaryAnchorMaxEnd = new Vector2(0f, 1f);
    #endregion

    private void OnEnable()
    {
        JukeboxManager.Instance.OnPreviewStart += StartSpinDisk;
        JukeboxManager.Instance.OnPreviewEnd += StopSpinDisk;
    }

    private void OnDisable()
    {
        JukeboxManager.Instance.OnPreviewStart -= StartSpinDisk;
        JukeboxManager.Instance.OnPreviewEnd -= StopSpinDisk;

        StopSpinDisk();
    }

    #region Base
    public void SetDisk(Sprite sprite)
    {
        if (sprite == _mainDiskImage.sprite) return;

        if (_useAnimation)
            StartCoroutine(SwitchDiskViaAnimation(sprite, null));
        else
            _mainDiskImage.sprite = sprite;
    }

    public void StartSpinDisk()
    {
        if (_diskSpinCoroutine != null) return;

        _diskSpinCoroutine = StartCoroutine(SpinDisk());
        return;
    }

    public void StopSpinDisk()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
        }

        _mainDiskImage.transform.rotation = Quaternion.identity;
    }

    public void ClearDisk() { StopSpinDisk(); _mainDiskImage.sprite = _emptyDiskSprite; }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _mainDiskImage.transform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);

            yield return null;
        }
    }
    #endregion

    #region Animation
    public IEnumerator SwitchDiskViaAnimation(Sprite sprite, System.Action<bool> done)
    {
        bool? diskSwitchDone = null;

        _secondaryDiskImage.sprite = _mainDiskImage.sprite;
        _mainDiskImage.sprite = sprite;

        _mainDiskRectTransform.anchorMin = _mainAnchorMinStart;
        _mainDiskRectTransform.anchorMax = _mainAnchorMaxStart;

        _secondaryDiskRectTransform.anchorMin = _secondaryAnchorMinStart;
        _secondaryDiskRectTransform.anchorMax = _secondaryAnchorMaxStart;

        StopSpinDisk();

        StartCoroutine(SwitchDisk((data) => diskSwitchDone = data));

        yield return new WaitUntil(() => diskSwitchDone != null);

        if (done != null) done(true);
    }

    private IEnumerator SwitchDisk(System.Action<bool> done)
    {
        float timer = 0f;

        while (timer < _switchDiskAnimDuration)
        {
            yield return null;

            timer += Time.deltaTime;

            //Disk switch
            float animatedFloat = _switchDiskAnimCurve.Evaluate(timer / _switchDiskAnimDuration);

            _mainDiskRectTransform.anchorMin = Vector2.Lerp(_mainAnchorMinStart, _mainAnchorMinEnd, animatedFloat);
            _mainDiskRectTransform.anchorMax = Vector2.Lerp(_mainAnchorMaxStart, _mainAnchorMaxEnd, animatedFloat);

            _secondaryDiskRectTransform.anchorMin = Vector2.Lerp(_secondaryAnchorMinStart, _secondaryAnchorMinEnd, animatedFloat);
            _secondaryDiskRectTransform.anchorMax = Vector2.Lerp(_secondaryAnchorMaxStart, _secondaryAnchorMaxEnd, animatedFloat);
        }

        done(true);
    }
    #endregion
}
