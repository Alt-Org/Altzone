using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ContentSizeFitter))]
public class TextAutoScroll : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _scrollSpeed = 0.3f;
    [SerializeField] private float _edgeWaitTime = 1f;

    private Coroutine _contentSetCoroutine;
    private Coroutine _scrollCoroutine;
    private Coroutine _waitCoroutine;

    private TMP_Text _text;
    private RectTransform _selfRect;
    private RectTransform _parentRect;
    private ContentSizeFitter _contentSizeFitter;
    private float _scrollProgress = 0f;
    private float _scrollDirection = 1f;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _selfRect = GetComponent<RectTransform>();
        _parentRect = transform.parent.GetComponent<RectTransform>();
        _contentSizeFitter = GetComponent<ContentSizeFitter>();
        _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _text.text = "";
    }

    private void OnEnable()
    {
        _contentSetCoroutine = StartCoroutine(ContentSet());
    }

    private void OnDisable()
    {
        DisableCoroutines();
    }

    private void DisableCoroutines()
    {
        if (_scrollCoroutine != null) StopCoroutine(_scrollCoroutine);

        if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);

        if (_contentSetCoroutine != null) StopCoroutine(_contentSetCoroutine);
    }

    public void ContentChange()
    {
        DisableCoroutines();
        _contentSetCoroutine = StartCoroutine(ContentSet());
    }

    private IEnumerator ContentSet()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(_text.text));

        if (_selfRect.sizeDelta.x > _parentRect.sizeDelta.x) _scrollCoroutine = StartCoroutine(Scroll());
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

            _selfRect.pivot = new Vector2(progress, 0.5f);
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
