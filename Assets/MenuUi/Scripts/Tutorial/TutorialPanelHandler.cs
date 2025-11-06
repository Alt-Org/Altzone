using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelHandler : MonoBehaviour
{
    [SerializeField] private Button _tutorialAdvanceButton;
    [SerializeField] private Image _imageToCutOut;
    [SerializeField] private GameObject _cutOut;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _infoBox;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private GameObject _fadeLayer;

    public void SetData(Action advanceAction)
    {
        if(_tutorialAdvanceButton != null)
            _tutorialAdvanceButton.onClick.AddListener(advanceAction.Invoke);

        if(_cutOut != null && _imageToCutOut != null)
        {
            _cutOut.GetComponent<Image>().sprite = _imageToCutOut.sprite;
            _cutOut.GetComponent<Image>().preserveAspect = _imageToCutOut.preserveAspect;

        }

        StartCoroutine(SetPosition());
    }

    private void OnEnable()
    {
        StartCoroutine(SetPosition());
    }

    public void UpdatePosition()
    {
        StartCoroutine(SetPosition());
    }

    private IEnumerator SetPosition()
    {
        yield return new WaitForEndOfFrame();
        if (_cutOut != null && _imageToCutOut != null)
        {
            _cutOut.GetComponent<RectTransform>().anchorMin = new(0.5f, 0.5f);
            _cutOut.GetComponent<RectTransform>().anchorMax = new(0.5f, 0.5f);
            _cutOut.transform.position = _imageToCutOut.transform.position;
            _cutOut.GetComponent<RectTransform>().sizeDelta = new(_imageToCutOut.GetComponent<RectTransform>().rect.width, _imageToCutOut.GetComponent<RectTransform>().rect.height);

            float screenWidth = _fadeLayer.GetComponent<RectTransform>().rect.width;
            float screenHeight = _fadeLayer.GetComponent<RectTransform>().rect.height;

            float widththreshold = screenWidth * 0.25f;

            float cutoutRightEdge = screenWidth / 2 + _cutOut.transform.position.x + _cutOut.GetComponent<RectTransform>().sizeDelta.x / 2;

            if ((screenWidth - cutoutRightEdge) < widththreshold)
            {
                float cutoutLeftEdge = screenWidth / 2 + _cutOut.transform.position.x - _cutOut.GetComponent<RectTransform>().sizeDelta.x / 2;

                if ((cutoutLeftEdge) < widththreshold)
                {
                    FlipInfo();
                }
                else
                {
                    if (screenWidth - cutoutRightEdge < cutoutLeftEdge)
                    {
                        FlipInfo();
                    }
                }
            }
        }
    }

    private void FlipInfo(bool flip = true)
    {
        if (flip)
        {
            _arrow.GetComponent<RectTransform>().anchorMin = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().anchorMax = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().pivot = new(1, 0.5f);
            _arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0,180,0);
            _infoText.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            _arrow.GetComponent<RectTransform>().anchorMin = new(1, 0.5f);
            _arrow.GetComponent<RectTransform>().anchorMax = new(1, 0.5f);
            _arrow.GetComponent<RectTransform>().pivot = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
            _infoText.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
        }
            _arrow.transform.localPosition = Vector3.zero;
    }
}
