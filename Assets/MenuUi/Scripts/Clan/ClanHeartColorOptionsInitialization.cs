using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanHeartColorOptionsInitialization : MonoBehaviour
{
    [SerializeField] private GameObject[] _colorButtons;

    private Image _buttonImage;
    private int _index = 0;

    private void Start()
    {
        foreach (var colorButton in _colorButtons)
        {
            _buttonImage = colorButton.GetComponent<Image>();

            switch (_index)
            {
                case 0:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Red);
                    break;
                case 1:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Orange);
                    break;
                case 2:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Yellow);
                    break;
                case 3:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Green);
                    break;
                case 4:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Turquoise);
                    break;
                case 5:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Indigo);
                    break;
                case 6:
                    _buttonImage.color = ColorConstants.GetColorConstant(ColorType.Violet);
                    break;
                default:
                    _buttonImage.color = Color.white;
                    break;
            }
            
            _index++;
        }
    }
}
