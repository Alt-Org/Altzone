using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerAnim : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _animationDuration = 0.2f;

    public void OpenPopup()
    {
        transform.localScale = new Vector3(0f, 0.5f, 1f);
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateOpen());
    }

    public void ClosePopup()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateOpen()
    {
        _canvasGroup.interactable = false;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one;

        float startAlpha = _canvasGroup.alpha;

        while (elapsed < _animationDuration)
        {
            float percent = elapsed / _animationDuration;
            float curve = Mathf.SmoothStep(0, 1, percent);

            transform.localScale = Vector3.Lerp(startScale, endScale, curve);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curve);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
    }

    private IEnumerator AnimateClose()
    {
        _canvasGroup.interactable = false;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = new(0f, 0.5f, 1f);

        float startAlpha = _canvasGroup.alpha;

        while (elapsed < _animationDuration)
        {
            float percent = elapsed / _animationDuration;
            float curve = Mathf.SmoothStep(0, 1, percent);

            transform.localScale = Vector3.Lerp(startScale, endScale, curve);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curve);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
