using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelHandler : MonoBehaviour
{
    [SerializeField] private Button _tutorialAdvanceButton;
    [SerializeField] private List<Image> _imageToCutOut;
    [SerializeField] private GameObject _cutOut;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _infoBox;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private GameObject _fadeLayer;

    private List<GameObject> _cutOuts = new();

    public void SetData(Action advanceAction)
    {
        if(_tutorialAdvanceButton != null)
            _tutorialAdvanceButton.onClick.AddListener(advanceAction.Invoke);

        _cutOuts.Clear();

        _cutOuts.Add(_cutOut);

        for (int i =1; i < _imageToCutOut.Count ;i++)
        {
            GameObject newCutOut = Instantiate(_cutOut, transform);
            _cutOuts.Add(newCutOut);
        }

        _fadeLayer.transform.SetAsLastSibling();
        int j = 0;
        foreach (Image imageToCutOut in _imageToCutOut)
        {
            if (_cutOuts[j] != null && imageToCutOut != null)
            {
                Image cutout = _cutOuts[j].GetComponent<Image>();
                cutout.sprite = imageToCutOut.sprite;
                cutout.preserveAspect = imageToCutOut.preserveAspect;
            }
            j++;
        }
        if (gameObject.activeSelf) StartCoroutine(SetPosition());
    }

    private void OnEnable()
    {
        StartCoroutine(SetPosition());
    }

    public void UpdatePosition()
    {
        if (gameObject.activeSelf) StartCoroutine(SetPosition());
    }

    private IEnumerator SetPosition()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < _cutOuts.Count; i++)
        {
            GameObject cutOut = _cutOuts[i];
            Image imageToCutOut = _imageToCutOut[i];
            if (!imageToCutOut.gameObject.activeInHierarchy) { cutOut.SetActive(false); continue; }
            else cutOut.SetActive(true);
            if (cutOut != null && imageToCutOut != null)
            {
                cutOut.GetComponent<RectTransform>().anchorMin = new(0.5f, 0.5f);
                cutOut.GetComponent<RectTransform>().anchorMax = new(0.5f, 0.5f);
                cutOut.GetComponent<RectTransform>().pivot = imageToCutOut.GetComponent<RectTransform>().pivot;
                cutOut.transform.position = imageToCutOut.transform.position;
                cutOut.GetComponent<RectTransform>().sizeDelta = new(imageToCutOut.GetComponent<RectTransform>().rect.width, imageToCutOut.GetComponent<RectTransform>().rect.height);

                float screenWidth = _fadeLayer.GetComponent<RectTransform>().rect.width;
                float screenHeight = _fadeLayer.GetComponent<RectTransform>().rect.height;

                float widththreshold = screenWidth * 0.25f;

                float cutoutRightEdge = screenWidth / 2 + cutOut.transform.localPosition.x + _cutOut.GetComponent<RectTransform>().sizeDelta.x / 2;

                if ((screenWidth - cutoutRightEdge) < widththreshold)
                {
                    float cutoutLeftEdge = screenWidth / 2 + cutOut.transform.localPosition.x - _cutOut.GetComponent<RectTransform>().sizeDelta.x / 2;

                    if ((cutoutLeftEdge) < widththreshold)
                    {
                        FlipInfo();
                        if (cutoutLeftEdge < 300) _arrow.GetComponent<RectTransform>().anchoredPosition = new(300 - cutoutLeftEdge, 0);
                    }
                    else
                    {
                        if (screenWidth - cutoutRightEdge < cutoutLeftEdge)
                        {
                            FlipInfo();
                            if (cutoutLeftEdge < 300) _arrow.GetComponent<RectTransform>().anchoredPosition = new(300 - cutoutLeftEdge, 0);
                        }
                        else
                        {
                            if (screenWidth - cutoutRightEdge < 300) _arrow.GetComponent<RectTransform>().anchoredPosition = new(-300 + (screenWidth - cutoutRightEdge), 0);
                        }
                    }

                }
                else
                {
                    if (screenWidth - cutoutRightEdge < 300) _arrow.GetComponent<RectTransform>().anchoredPosition = new(-300 + (screenWidth - cutoutRightEdge), 0);
                }
            }
            yield return null;
        }

        GameObject previousArrow = null;
        for (int i = 0; i < _cutOuts.Count; i++)
        {
            if (!_cutOuts[i].activeInHierarchy) continue;
            if(previousArrow) previousArrow.SetActive(false);
            previousArrow =_cutOuts[i].transform.Find("Arrow").gameObject;
        }
    }

    private void FlipInfo(bool flip = true)
    {
        if (flip)
        {
            _arrow.GetComponent<RectTransform>().anchorMin = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().anchorMax = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().pivot = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0,180,0);
            _infoText.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            _arrow.GetComponent<RectTransform>().anchorMin = new(1, 0.5f);
            _arrow.GetComponent<RectTransform>().anchorMax = new(1, 0.5f);
            _arrow.GetComponent<RectTransform>().pivot = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
            _infoText.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
