using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Audio
{
    [RequireComponent(typeof(ContentSizeFitter))]
    public class TextAutoScroll : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float _defaultHorizontalPosition = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float _horizontalStartPosition = 0f;
        [Range(0f, 1f)]
        [SerializeField] private float _verticalPosition = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float _scrollSpeed = 0.3f;
        [SerializeField] private float _edgeWaitTime = 1f;
        [Header("Optional")]
        [SerializeField] private Fade _fade = null;
        [SerializeField] private TextStyleCloner _textStyleCloner = null;

        private Coroutine _contentSetCoroutine;
        private Coroutine _scrollCoroutine;
        private Coroutine _waitCoroutine;

        private Coroutine _fadeOutCoroutine;
        private Coroutine _fadeInCoroutine;

        private TMP_Text _text;
        private RectTransform _selfRect;
        private RectTransform _parentRect;
        private ContentSizeFitter _contentSizeFitter;
        private float _scrollProgress = 0f;
        private float _scrollDirection = -1f;
        private string _textInQueue = null;

        private bool _textStyleUpdated = false;
        private bool _textFadeActive = false;

        void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _selfRect = GetComponent<RectTransform>();
            _parentRect = transform.parent.GetComponent<RectTransform>();
            _contentSizeFitter = GetComponent<ContentSizeFitter>();
            _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void OnEnable()
        {
            if (_text == null || string.IsNullOrEmpty(_textInQueue) || _textFadeActive) return;

            DisableCoroutines();

            if (_text.text == _textInQueue && _text.color.a == 1f)
            {
                TryStartScroll();

                return;
            }

            _contentSetCoroutine = StartCoroutine(ContentSet());

            //SetContent(_textInQueue);
        }

        private void OnDisable() { DisableCoroutines(); }

        public void DisableCoroutines()
        {
            DisableCoroutine(ref _scrollCoroutine);
            DisableCoroutine(ref _waitCoroutine);
            DisableCoroutine(ref _contentSetCoroutine);
            _textFadeActive = false;
        }

        private void DisableCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public void SetContent(string text, bool forceStart = false)
        {
            if (!forceStart && _text && _text.text == text || _textInQueue == text)
            {
                TryStartScroll();

                return;
            }

            DisableCoroutines();
            _textInQueue = text;

            if (isActiveAndEnabled && !_textFadeActive) _contentSetCoroutine = StartCoroutine(ContentSet());
        }

        private IEnumerator ContentSet()
        {
            bool? fadeOperationDone = null;

            _textFadeActive = true;

            if (_fade != null && !string.IsNullOrEmpty(_text.text))
            {
                _fade.Reset();

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);
                    _fadeOutCoroutine = null;
                }

                _fadeOutCoroutine = StartCoroutine(_fade.FadeOperation(Fade.FadeType.Out, (data) => fadeOperationDone = data, true));
            }
            else if (_fade != null)
            {
                _fade.SetAlphaVisibility(false);
                fadeOperationDone = true;
            }

            yield return new WaitUntil(() => (_fade == null || fadeOperationDone != null));

            _fadeOutCoroutine = null;
            _text.text = _textInQueue;
            _scrollDirection = -1f;

            yield return new WaitForEndOfFrame();

            if (!_textStyleUpdated && _textStyleCloner != null)
            {
                _textStyleUpdated = true;
                _textStyleCloner.StartCorrection(true);
            }

            _selfRect.pivot = new Vector2((_selfRect.sizeDelta.x > 0f ? _horizontalStartPosition : _defaultHorizontalPosition), _verticalPosition);

            if (_fade != null)
            {
                fadeOperationDone = null;

                if (_fadeInCoroutine != null)
                {
                    StopCoroutine(_fadeInCoroutine);
                    _fadeInCoroutine = null;
                }

                _fadeInCoroutine = StartCoroutine(_fade.FadeOperation(Fade.FadeType.In, (data) => fadeOperationDone = data, true));

                yield return new WaitUntil(() => fadeOperationDone != null);

                _fadeInCoroutine = null;
            }

            _textFadeActive = false;

            TryStartScroll();
        }

        private void TryStartScroll()
        {
            if (_scrollCoroutine != null || _selfRect == null || !isActiveAndEnabled || _textFadeActive) return;

            DisableCoroutine(ref _waitCoroutine);

            if (_selfRect.sizeDelta.x <= 0f)
            {
                DisableCoroutine(ref _scrollCoroutine);

                return;
            }

            _selfRect.pivot = new Vector2(_horizontalStartPosition, _verticalPosition);
            _scrollDirection = -1f;
            _waitCoroutine = StartCoroutine(Wait());
        }

        private IEnumerator Scroll()
        {
            float targetValue = _scrollDirection == 1 ? 1 : 0;
            float progress = _scrollDirection == 1 ? 0 : 1;

            while (Valid(progress, targetValue))
            {
                yield return null;

                progress += (_scrollSpeed * _scrollDirection) * Time.deltaTime;

                if (!Valid(progress, targetValue)) progress = targetValue;

                _selfRect.pivot = new Vector2(Mathf.Lerp(0f, 1f, progress), _verticalPosition);
            }

            _waitCoroutine = StartCoroutine(Wait());
        }

        private bool Valid(float progress, float target)
        {
            if (target == 1)
                return progress < target;
            else
                return progress > target;
        }

        private IEnumerator Wait()
        {
            float timer = 0f;

            while (timer < _edgeWaitTime)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            _scrollDirection *= -1;
            _scrollCoroutine = StartCoroutine(Scroll());
        }
    }
}
