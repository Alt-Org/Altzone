using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Audio
{
    public class Fade : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 1f;

        private TMPro.TextMeshProUGUI _textMeshProUGUI;
        private Image _image;

        private Coroutine _fadeCoroutine;

        private bool _inUse = false;
        public bool InUse { get { return _inUse; } }

        public enum FadeType
        {
            In,
            Out
        }

        private void Awake()
        {
            _textMeshProUGUI = GetComponent<TMPro.TextMeshProUGUI>();
            _image = GetComponent<Image>();
        }

        public void Reset()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_textMeshProUGUI != null) _textMeshProUGUI.alpha = 1f;

            if (_image != null)
            {
                Color tempColor = _image.color;
                tempColor.a = 1f;
                _image.color = tempColor;
            }
        }

        public void StartFadeOperation(FadeType type, bool forceOneWay = false)
        {
            _inUse = true;
            Reset();
            StartCoroutine(FadeOperation(type, (data) => _inUse = !data, forceOneWay));
        }

        public IEnumerator StartFullFadeOperation(System.Action<bool> fadeOutDone, System.Action<bool> fadeInDone, bool forceOneWay = false)
        {
            bool? result = null;

            _inUse = true;
            Reset();
            StartCoroutine(FadeOperation(FadeType.Out, (data) => result = data, forceOneWay));

            yield return new WaitUntil(() => result != null);

            fadeOutDone(true);
            result = null;
            StartCoroutine(FadeOperation(FadeType.In, (data) => result = data, forceOneWay));

            yield return new WaitUntil(() => result != null);

            _inUse = false;
            fadeInDone(true);
        }

        public IEnumerator FadeOperation(FadeType type, System.Action<bool> done = null, bool forceOneWay = false)
        {
            float timer = 0f;

            while (timer < _fadeDuration)
            {
                yield return null;
                timer += Time.deltaTime;

                float progress = type == FadeType.Out ? Mathf.Lerp(1f, 0f, timer / _fadeDuration) : Mathf.Lerp(0f, 1f, timer / _fadeDuration);

                if (_textMeshProUGUI != null) _textMeshProUGUI.alpha = progress;

                if (_image != null)
                {
                    Color tempColor = _image.color;
                    tempColor.a = progress;
                    _image.color = tempColor;
                }
            }

            done(true);
        }

        public void SetAlphaVisibility(bool visible)
        {
            float progress = visible ? 1f : 0f;

            if (_textMeshProUGUI != null) _textMeshProUGUI.alpha = progress;

            if (_image != null)
            {
                Color tempColor = _image.color;
                tempColor.a = progress;
                _image.color = tempColor;
            }
        }
    }
}
