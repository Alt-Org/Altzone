using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ToggleSliderColor : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _checkMark;
    [SerializeField] private RectTransform Handler;
    [SerializeField] private Color _offcolor;
    [SerializeField] private Color _oncolor;
    private Vector2 _offtoggle = new Vector2(0, 0);
    private Vector2 _ontoggle = new Vector2(45, 0);

    private void Start()
    {
        _backgroundImage.color = _offcolor;
        Handler.anchoredPosition = _offtoggle;
    }
        ///This is used to change the sliders outlook,
        ///Mainly because there's no reason for this slider to have slider function when it just works same as toggle would normally
    public void ToggleSystem(bool IsOn)
    {


        _backgroundImage.color = IsOn ? _oncolor  :_offcolor; 

        Handler.anchoredPosition = IsOn ? _ontoggle : _offtoggle;

        if(_checkMark != null)
        _checkMark.enabled = IsOn ? false : true;
    }





}
