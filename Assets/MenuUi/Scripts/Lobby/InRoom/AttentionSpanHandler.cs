using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttentionSpanHandler : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _borders;
    [SerializeField] private RectTransform _animationContent;
    [SerializeField] private TMP_Text _text1;
    [SerializeField] private TMP_Text _text2;
    [SerializeField] private Animator _animation;
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _topSection;

    [SerializeField] private Button _closeButton;

    private void Start()
    {
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        Rect spriteRect = _image.sprite.textureRect;
        float width = spriteRect.width;
        float height = spriteRect.height;
        float ratio = width / height;
        _image.GetComponent<AspectRatioFitter>().aspectRatio = ratio;

        float contentAreaHeight = _animationContent.rect.height;
        float animationWindowHeight = _image.GetComponent<RectTransform>().rect.height;
        _topSection.sizeDelta = new Vector2(_topSection.sizeDelta.x, contentAreaHeight-animationWindowHeight);

    }
    public void OpenPanel()
    {
        _panel.SetActive(true);
        _animationContent.gameObject.SetActive(true);
    }

    private void ClosePanel()
    {
        _panel.SetActive(false);
    }
}
