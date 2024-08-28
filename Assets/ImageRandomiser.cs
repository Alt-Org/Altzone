using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ImageRandomiser : MonoBehaviour
{
    [SerializeField]private List<Sprite> _sprites = new List<Sprite>();

    private int _prevImageValue = -1;
    private Image _image = null;
    private Button _button = null;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        if(_button != null) _button.onClick.AddListener(RandomiseImage);
    }

    private void OnEnable()
    {
        if (_image != null) RandomiseImage();
    }

    public void RandomiseImage()
    {
        if (_image == null) return;
        if (_sprites.Count <= 0) return;
        int newValue;
        do
        {
            newValue = Random.Range(0, _sprites.Count);
        }
        while (newValue == _prevImageValue);
        _image.sprite = _sprites[newValue];
        _prevImageValue = newValue;
    }
}
