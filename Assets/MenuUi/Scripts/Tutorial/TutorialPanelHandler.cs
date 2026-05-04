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
    [SerializeField] private CutOutHandler _cutOut;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _infoBox;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private GameObject _fadeLayer;

    private List<CutOutHandler> _cutOuts = new();

    public void SetData(Action advanceAction)
    {
        if(_tutorialAdvanceButton != null)
            _tutorialAdvanceButton.onClick.AddListener(advanceAction.Invoke);

        _cutOuts.Clear();

        _cutOuts.Add(_cutOut);

        for (int i =1; i < _imageToCutOut.Count ;i++)
        {
            CutOutHandler newCutOut = Instantiate(_cutOut, transform);
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
        for (int i = 0; i < _cutOuts.Count; i++)
        {
            _cutOuts[i].SetPosition(_imageToCutOut[i], _fadeLayer);
        }
        yield return null;
        GameObject previousArrow = null;
        for (int i = 0; i < _cutOuts.Count; i++)
        {
            if (!_cutOuts[i].gameObject.activeInHierarchy || _cutOuts[i].KeepArrowActive) continue;
            if(previousArrow) previousArrow.SetActive(false);
            previousArrow =_cutOuts[i].Arrow;
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
