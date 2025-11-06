using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelHandler : MonoBehaviour
{
    [SerializeField] private Button _tutorialAdvanceButton;
    [SerializeField] private Image _imageToCutOut;
    [SerializeField] private GameObject _cutOut;

    public void SetData(Action advanceAction)
    {
        if(_tutorialAdvanceButton != null)
            _tutorialAdvanceButton.onClick.AddListener(advanceAction.Invoke);

        if(_cutOut != null)
        {
            _cutOut.GetComponent<RectTransform>().anchorMin = new(0.5f, 0.5f);
            _cutOut.GetComponent<RectTransform>().anchorMax = new(0.5f, 0.5f);
            _cutOut.transform.position = _imageToCutOut.transform.position;
            _cutOut.GetComponent<RectTransform>().sizeDelta = new(_imageToCutOut.GetComponent<RectTransform>().rect.width, _imageToCutOut.GetComponent<RectTransform>().rect.height);
            _cutOut.GetComponent<Image>().sprite = _imageToCutOut.sprite;
            _cutOut.GetComponent<Image>().preserveAspect = _imageToCutOut.preserveAspect;
        }
    }

    private void Update()
    {
        _cutOut.transform.position = _imageToCutOut.transform.position;
        _cutOut.GetComponent<RectTransform>().sizeDelta = new(_imageToCutOut.GetComponent<RectTransform>().rect.width, _imageToCutOut.GetComponent<RectTransform>().rect.height);
    }

}
