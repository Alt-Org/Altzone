using System.Collections;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorCellHandler : GridCellHandler
    {
        [SerializeField] private RectTransform _visualRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        public RectTransform VisualRoot => _visualRoot;

        private Coroutine _animRoutine;

        private void OnEnable()
        {
            _visualRoot.anchoredPosition = new Vector2(-100f, 0);
            _visualRoot.localScale = new Vector3(0.7f, 0.7f, 1f);
            _canvasGroup.alpha = 0;

            if (_animRoutine != null) StopCoroutine(_animRoutine);

            _animRoutine = StartCoroutine(AnimateIn());
        }

        public void StartCloseAnimation(float duration)
        {
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            StartCoroutine(AnimateOut(duration));
        }

        private IEnumerator AnimateIn()
        {
            int index = transform.GetSiblingIndex();

            yield return new WaitForSeconds(index * 0.04f);

            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / duration;
                float curve = Mathf.SmoothStep(0, 1, percent);

                _visualRoot.anchoredPosition = Vector2.Lerp(new Vector2(-100f, 0), Vector2.zero, curve);
                _visualRoot.localScale = Vector3.Lerp(new Vector3(0.7f, 0.7f, 1f), Vector3.one, curve);
                _canvasGroup.alpha = curve;

                yield return null;
            }

            _visualRoot.anchoredPosition = Vector2.zero;
            _visualRoot.localScale = Vector3.one;
            _canvasGroup.alpha = 1;
        }

        private IEnumerator AnimateOut(float duration)
        {
            float elapsed = 0f;
            Vector3 startScale = _visualRoot.localScale;
            Vector2 startPos = _visualRoot.anchoredPosition;
            float startalpha = _canvasGroup.alpha;

            Vector2 targetPos = new Vector2(100f, 0);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / duration;

                float curve = percent * percent;

                _visualRoot.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);
                _visualRoot.localScale = Vector3.Lerp(startScale, new Vector3(0.5f, 0.5f, 1f), curve);
                _canvasGroup.alpha = Mathf.Lerp(startalpha, 0, curve);
                yield return null;
            }
        }

        public void SetColor(Color color)
        {
           base._featureImage.color = color;
        }

        public void SetOnClick(UnityEngine.Events.UnityAction onClick)
        {
            base.SetValues(onClick: onClick);
        }
    }
}
