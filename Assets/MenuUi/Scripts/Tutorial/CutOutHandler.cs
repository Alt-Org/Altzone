using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutOutHandler : MonoBehaviour
{
    [SerializeField] private RectTransform _rect;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private bool _keepArrowActive = false;

    public GameObject Arrow { get => _arrow; }
    public bool KeepArrowActive { get => _keepArrowActive; }

    public void SetPosition(Image imageToCutOut, GameObject fadeLayer)
    {
        if (!imageToCutOut.gameObject.activeInHierarchy) { gameObject.SetActive(false); return; }
        else gameObject.SetActive(true);
        if (gameObject != null && imageToCutOut != null)
        {
            _rect.anchorMin = new(0.5f, 0.5f);
            _rect.anchorMax = new(0.5f, 0.5f);
            _rect.pivot = imageToCutOut.GetComponent<RectTransform>().pivot;
            transform.position = imageToCutOut.transform.position;
            _rect.sizeDelta = new(imageToCutOut.GetComponent<RectTransform>().rect.width, imageToCutOut.GetComponent<RectTransform>().rect.height);

            if (!_arrow.activeSelf || fadeLayer == null) return;

            float screenWidth = fadeLayer.GetComponent<RectTransform>().rect.width;
            float screenHeight = fadeLayer.GetComponent<RectTransform>().rect.height;

            float widththreshold = screenWidth * 0.25f;

            float cutoutRightEdge = screenWidth / 2 + _rect.transform.localPosition.x + _rect.GetComponent<RectTransform>().sizeDelta.x / 2;

            if ((screenWidth - cutoutRightEdge) < widththreshold)
            {
                float cutoutLeftEdge = screenWidth / 2 + _rect.transform.localPosition.x - _rect.GetComponent<RectTransform>().sizeDelta.x / 2;

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
    }

    private void FlipInfo(bool flip = true)
    {
        if (flip)
        {
            _arrow.GetComponent<RectTransform>().anchorMin = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().anchorMax = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().pivot = new(0, 0.5f);
            _arrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
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
