using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class IntroNextButtonHandler : MonoBehaviour
{
    [SerializeField] private RectTransform _nextButtonText;
    [SerializeField] private RectTransform _nextButtonBase;
    [SerializeField] private RectTransform _nextButton;

    private void OnEnable()
    {
        StartCoroutine(RecalculateWidth());
    }

    private IEnumerator RecalculateWidth()
    {
        Debug.LogWarning("Recalculating");
        yield return new WaitForEndOfFrame();
        float buttonImageWidth = _nextButtonBase.rect.width;
        float buttonTextWidth = _nextButtonText.rect.width;
        float wholeWidth = buttonImageWidth + buttonTextWidth;
        _nextButton.sizeDelta = new(wholeWidth, 0);
        _nextButtonText.anchoredPosition = new(-buttonImageWidth, 0);
    }
}
