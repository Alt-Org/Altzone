using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Settings.BattleUiEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SliderPopupHnadler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private OptionsPopup _optionsPopup;
    [SerializeField] private Transform _valueField;

    public void OnPointerDown(PointerEventData eventData)
    {
        _optionsPopup.HideExceptSlider(transform, _valueField);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _optionsPopup.RestoreAfterSliderDragging();
    }
}
