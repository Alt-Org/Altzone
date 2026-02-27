using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class PopAnimation : MonoBehaviour
{
    [Tooltip("Simple Pop ignores Pop Curve")]
    [SerializeField] private AnimationType _animType;
    [Tooltip("Animation Duration in seconds")]
    [SerializeField, Range(0.01f, 5f)] private float _duration = 0.3f;
    [Tooltip("Set between 0-1 seconds")]
    [SerializeField] private AnimationCurve _popCurve;
    [Tooltip("Alpha follows curve instead of increasing linearly with duration")]
    [SerializeField] private bool _alphaFollowsCurve = false;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private enum AnimationType { SimplePop, CurvePop }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();

        switch(_animType)
        {
            case AnimationType.SimplePop:
                StartCoroutine(SimplePop());
                break;
            case AnimationType.CurvePop:
                StartCoroutine(AnimatePop());
                break;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AnimatePop()
    {
        float elapsed = 0f;
        _rectTransform.localScale = Vector3.zero;
        _canvasGroup.alpha = 0;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _duration;

            float curveValue = _popCurve.Evaluate(percent);

            _rectTransform.localScale = Vector3.one * curveValue;

            if (_alphaFollowsCurve)
                _canvasGroup.alpha = Mathf.Clamp01(curveValue);
            else
                _canvasGroup.alpha = percent;

                yield return null;
        }
        _rectTransform.localScale = Vector3.one;
        _canvasGroup.alpha = 1;
    }

    private IEnumerator SimplePop()
    {
        float elapsed = 0f;
        _canvasGroup.alpha = 0;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _duration;
            _canvasGroup.alpha = percent;

            if (percent < 0.7f)
            {
                float t = percent / 0.7f;
                _rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.15f, t);
            }
            else
            {
                float t = (percent - 0.7f) / 0.3f;
                _rectTransform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t);
            }

            yield return null;
        }
        _rectTransform.localScale = Vector3.one;
        _canvasGroup.alpha = 1;
    }
}
