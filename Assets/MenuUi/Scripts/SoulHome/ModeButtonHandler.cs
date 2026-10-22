using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeButtonHandler : MonoBehaviour
{
    [SerializeField] private Image _decorateModeImage;
    [SerializeField] private Image _renovationModeImage;
    private bool _isOnDecorateMode = true;
    // Start is called before the first frame update
    public void ToggleMode()
    {
        _isOnDecorateMode = !_isOnDecorateMode;
        if (_isOnDecorateMode)
        {
            _decorateModeImage.gameObject.SetActive(true);
            _renovationModeImage.gameObject.SetActive(false);
        }
        else
        {
            _decorateModeImage.gameObject.SetActive(false);
            _renovationModeImage.gameObject.SetActive(true);
        }
    }
}
