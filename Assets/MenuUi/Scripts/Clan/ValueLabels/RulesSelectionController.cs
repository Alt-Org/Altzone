using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesSelectionController : AltMonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _rulesSelectorObject;
    [SerializeField] private GameObject _activateButton;

    [SerializeField] private BaseScrollRect _scrollRectComponent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _labelTogglePrefab;
    [SerializeField] private GameObject _labelImagePrefab;

    [Header("Containers")]
    [SerializeField] private Transform _rulesListParent;
    [SerializeField] private TextMeshProUGUI _selectedRule1;
    [SerializeField] private TextMeshProUGUI _selectedRule2;
    [SerializeField] private TextMeshProUGUI _selectedRule3;
    private List<TextMeshProUGUI> _selectedRuleList;

    private List<RulesLabelHandler> _labelHandlers = new();
    public List<Rules> SelectedRules { get; private set; } = new();

    private void Start()
    {
        _selectedRuleList = new() { _selectedRule1, _selectedRule2, _selectedRule3 };
        CreateLabels();
        StartCoroutine(ResetScrollPosition());
    }

    public void SetSelected(List<Rules> selected)
    {
        if(_selectedRuleList == null) _selectedRuleList = new() { _selectedRule1, _selectedRule2, _selectedRule3 };
        SelectedRules = new(selected);
        CreateLabels();
        UpdateSelectedDisplay();
    }

    private IEnumerator ResetScrollPosition()
    {
        yield return null;
        if (_scrollRectComponent != null)
        {
            //_scrollRectComponent.verticalNormalizedPosition = 1f;
        }

    }

    private void CreateLabels()
    {
        _labelHandlers.Clear();

        foreach (Transform child in _rulesListParent) Destroy(child.gameObject);

        foreach (Rules value in Enum.GetValues(typeof(Rules)))
        {
            GameObject labelPanel = Instantiate(_labelTogglePrefab, _rulesListParent);
            RulesLabelHandler labelHandler = labelPanel.GetComponent<RulesLabelHandler>();
            labelHandler.SetLabelInfo(value);
            _labelHandlers.Add(labelHandler);

            if (SelectedRules.Contains(value)) labelHandler.Select();

            labelHandler._selectButton.onClick.AddListener(() => ToggleValue(labelHandler));
        }
    }

    public void ToggleValue(RulesLabelHandler toggledHandler)
    {
        if (SelectedRules.Contains(toggledHandler.labelData) && SelectedRules.Count == 0)
        {
            return;
        }


        if (SelectedRules.Contains(toggledHandler.labelData))
        {
            RulesLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelData == toggledHandler.labelData);
            handlerOfRemoved.Unselect();
            SelectedRules.Remove(toggledHandler.labelData);
        }
        else
        {
            if (SelectedRules.Count < 3)
            {
                SelectedRules.Add(toggledHandler.labelData);
                RulesLabelHandler handlerOfSelected = _labelHandlers.Find(handler => handler.labelData == toggledHandler.labelData);
                handlerOfSelected.Select();
            }
        }

        UpdateSelectedDisplay();
    }

    public void RemoveSelectedValue(RulesLabelHandler removedHandler)
    {
        if (SelectedRules.Count == 0)
        {
            return;
        }

        if (_rulesSelectorObject.activeSelf)
        {
            RulesLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelData == removedHandler.labelData);
            handlerOfRemoved.Unselect();
            SelectedRules.Remove(removedHandler.labelData);

            UpdateSelectedDisplay();
        }
        else
        {
            _activateButton.GetComponent<Button>().onClick.Invoke();
        }

    }

    private void UpdateSelectedDisplay()
    {
        foreach (TextMeshProUGUI child in _selectedRuleList) child.text = "";
        int i = 0;
        foreach (Rules value in SelectedRules)
        {
            _selectedRuleList[i].SetText(ClanDataTypeConverter.GetRulesText(value));
            i++;
        }
    }

    public void ResetSelection()
    {
        SelectedRules.Clear();
        CreateLabels();
        UpdateSelectedDisplay();
        StartCoroutine(ResetScrollPosition());
    }
}
