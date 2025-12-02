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

        void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _selfRect = GetComponent<RectTransform>();
            _parentRect = transform.parent.GetComponent<RectTransform>();
            _contentSizeFitter = GetComponent<ContentSizeFitter>();
            _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            //_text.text = "";
        }

        private void OnEnable()
        {
            _contentSetCoroutine = StartCoroutine(ContentSet());
        }

        private void OnDisable()
        {
            DisableCoroutines();
        }

        public void DisableCoroutines()
        {
            if (_scrollCoroutine != null) StopCoroutine(_scrollCoroutine);

            if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);

            if (_contentSetCoroutine != null) StopCoroutine(_contentSetCoroutine);
        }

        public void SetContent(string text, bool forceStart = false)
        {
            if (!forceStart && _text != null && _text.text == text) return;

            _textInQueue = text;
            DisableCoroutines();

            if (isActiveAndEnabled) _contentSetCoroutine = StartCoroutine(ContentSet());
        }

        private IEnumerator ContentSet()
        {
            bool? fadeOperationDone = null;

            if (_fade != null)
            {
                _fade.Reset();

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);
                    _fadeOutCoroutine = null;
                }

                _fadeOutCoroutine = StartCoroutine(_fade.FadeOperation(Fade.FadeType.Out, (data) => fadeOperationDone = data, true));
            }

            yield return new WaitUntil(() => !string.IsNullOrEmpty(_textInQueue) && (_fade == null || (_fade != null && fadeOperationDone != null)));

            _text.text = _textInQueue;
            _textInQueue = null;
            _scrollDirection = -1f;

            yield return new WaitForEndOfFrame();

            if (_selfRect.sizeDelta.x > 0f)
                _selfRect.pivot = new Vector2(_horizontalStartPosition, _verticalPosition);
            else
                _selfRect.pivot = new Vector2(_defaultHorizontalPosition, _verticalPosition);

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
            }

            if (_selfRect.sizeDelta.x > 0f) _scrollCoroutine = StartCoroutine(Wait());
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
