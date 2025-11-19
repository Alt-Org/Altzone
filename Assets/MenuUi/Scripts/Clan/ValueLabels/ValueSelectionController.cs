using System;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.UI;
using System.Collections;

public class ValueSelectionController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _valueSelectorObject;
    [SerializeField] private GameObject _activateButton;

    [SerializeField] private ScrollRect _scrollRectComponent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _labelTogglePrefab;
    [SerializeField] private GameObject _labelImagePrefab;

    [Header("Containers")]
    [SerializeField] private Transform _valueListParent;
    [SerializeField] private Transform _selectedValuesParent;

    private List<ValueLabelHandler> _labelHandlers = new();
    public List<ClanValues> SelectedValues { get; private set; } = new();

    private void Start()
    {
        CreateLabels();
        StartCoroutine(ResetScrollPosition());
    }

    public void SetSelected(List<ClanValues> selected)
    {
        SelectedValues = new(selected);
        CreateLabels();
        UpdateSelectedDisplay();
    }

    private IEnumerator ResetScrollPosition()
    {
        yield return null;
        if(_scrollRectComponent != null)
        {
            _scrollRectComponent.verticalNormalizedPosition = 1f;
        }
        
    }

    private void CreateLabels()
    {
        _labelHandlers.Clear();

        foreach (Transform child in _valueListParent) Destroy(child.gameObject);

        foreach (ClanValues value in Enum.GetValues(typeof(ClanValues)))
        {
            GameObject labelPanel = Instantiate(_labelTogglePrefab, _valueListParent);
            ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();
            labelHandler.SetLabelInfo(value, true);
            _labelHandlers.Add(labelHandler);

            if (SelectedValues.Contains(value)) labelHandler.Select();

            labelHandler._selectButton.onClick.AddListener(() => ToggleValue(labelHandler));
        }
    }

    public void ToggleValue(ValueLabelHandler toggledHandler)
    {
        if(SelectedValues.Contains(toggledHandler.labelInfo.values) && SelectedValues.Count == 1)
        {
            return;
        }


        if (SelectedValues.Contains(toggledHandler.labelInfo.values))
        {
            ValueLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelInfo.values == toggledHandler.labelInfo.values);
            handlerOfRemoved.Unselect();
            SelectedValues.Remove(toggledHandler.labelInfo.values);
        }
        else
        {
            if (SelectedValues.Count < 3)
            {
                SelectedValues.Add(toggledHandler.labelInfo.values);
                ValueLabelHandler handlerOfSelected = _labelHandlers.Find(handler => handler.labelInfo.values == toggledHandler.labelInfo.values);
                handlerOfSelected.Select();
            }
        }

        UpdateSelectedDisplay();
    }

    public void RemoveSelectedValue(ValueLabelHandler removedHandler)
    {
        if(SelectedValues.Count == 1)
        {
            return;
        }

        if (_valueSelectorObject.activeSelf)
        {
            ValueLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelInfo.values == removedHandler.labelInfo.values);
            handlerOfRemoved.Unselect();
            SelectedValues.Remove(removedHandler.labelInfo.values);

            UpdateSelectedDisplay();
        }
        else
        {
            _activateButton.GetComponent<Button>().onClick.Invoke();
        }

    }

    private void UpdateSelectedDisplay()
    {
        foreach (Transform child in _selectedValuesParent) Destroy(child.gameObject);

        foreach (ClanValues value in SelectedValues)
        {
            GameObject selectedPanel = Instantiate(_labelTogglePrefab, _selectedValuesParent);
            ValueLabelHandler labelHandlerSelected = selectedPanel.GetComponent<ValueLabelHandler>();
            labelHandlerSelected.SetLabelInfo(value, false);
            _labelHandlers.Add(labelHandlerSelected);

            labelHandlerSelected._selectButton.onClick.AddListener(() => RemoveSelectedValue(labelHandlerSelected));
        }
    }
}
