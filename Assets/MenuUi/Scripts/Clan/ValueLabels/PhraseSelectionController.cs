using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhraseSelectionController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _phraseSelectorObject;
    [SerializeField] private GameObject _activateButton;

    [SerializeField] private BaseScrollRect _scrollRectComponent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _labelTogglePrefab;
    [SerializeField] private GameObject _labelImagePrefab;

    [Header("Containers")]
    [SerializeField] private Transform _phraseListParent;
    [SerializeField] private TextMeshProUGUI _selectedPhrase;
    private List<TextMeshProUGUI> _selectedPhraseList;

    [Header("Custom input")]
    [SerializeField] private TMP_InputField _targetPhraseInput;
    [SerializeField] private bool _clearSelectionWhenTyping = true;

    private List<PhraseLabelHandler> _labelHandlers = new();
    public List<Phrases> SelectedPhrases { get; private set; } = new();

    private void Start()
    {
        _selectedPhraseList = new() { _selectedPhrase };
        CreateLabels();

        if(_targetPhraseInput != null)
        {
            _targetPhraseInput.onValueChanged.RemoveListener(OnTargetInputChanged);
            _targetPhraseInput.onValueChanged.AddListener(OnTargetInputChanged);
        }

        StartCoroutine(ResetScrollPosition());
    }

    public void SetSelected(List<Phrases> selected)
    {
        if (_selectedPhraseList == null) _selectedPhraseList = new() { _selectedPhrase };
        SelectedPhrases = new(selected);
        CreateLabels();
        UpdateSelectedDisplay();

        ApplySelectedToInputField();
    }

    private void OnTargetInputChanged(string newText)
    {
        if (!_clearSelectionWhenTyping) return;
        if (SelectedPhrases.Count == 0) return;

        string selectedText = ClanDataTypeConverter.GetPhraseText(SelectedPhrases[0]);
        if (!string.Equals(newText, selectedText, StringComparison.Ordinal))
        {
            // Tyhjennä valinnat + päivitä UI
            foreach (var handler in _labelHandlers) handler.Unselect();
            SelectedPhrases.Clear();
            UpdateSelectedDisplay();
        }
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

        foreach (Transform child in _phraseListParent) Destroy(child.gameObject);

        foreach (Phrases value in Enum.GetValues(typeof(Phrases)))
        {
            GameObject labelPanel = Instantiate(_labelTogglePrefab, _phraseListParent);
            PhraseLabelHandler labelHandler = labelPanel.GetComponent<PhraseLabelHandler>();
            labelHandler.SetLabelInfo(value);
            _labelHandlers.Add(labelHandler);

            if (SelectedPhrases.Contains(value)) labelHandler.Select();

            labelHandler._selectButton.onClick.AddListener(() => ToggleValue(labelHandler));
        }
    }

    public void ToggleValue(PhraseLabelHandler toggledHandler)
    {
        if (SelectedPhrases.Contains(toggledHandler.labelData))
        {
            toggledHandler.Unselect();
            SelectedPhrases.Clear();
            UpdateSelectedDisplay();
            return;
        }

        foreach(var handler in _labelHandlers)
        {
            handler.Unselect();
        }

        SelectedPhrases.Clear();
        SelectedPhrases.Add(toggledHandler.labelData);
        toggledHandler.Select();

        UpdateSelectedDisplay();
        ApplySelectedToInputField();
    }

    public void RemoveSelectedValue(PhraseLabelHandler removedHandler)
    {
        if (SelectedPhrases.Count == 0)
        {
            return;
        }

        if (_phraseSelectorObject.activeSelf)
        {
            PhraseLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelData == removedHandler.labelData);
            handlerOfRemoved.Unselect();
            SelectedPhrases.Remove(removedHandler.labelData);

            UpdateSelectedDisplay();
        }
        else
        {
            _activateButton.GetComponent<Button>().onClick.Invoke();
        }

    }

    private void UpdateSelectedDisplay()
    {
        foreach (var child in _selectedPhraseList) child.text = "";

        if(SelectedPhrases.Count > 0)
        {
            _selectedPhraseList[0].SetText(ClanDataTypeConverter.GetPhraseText(SelectedPhrases[0]));
        }
    }

    private void ApplySelectedToInputField()
    {
        if (_targetPhraseInput == null) return;
        if (SelectedPhrases.Count == 0) return;

        string text = ClanDataTypeConverter.GetPhraseText(SelectedPhrases[0]);

        _targetPhraseInput.SetTextWithoutNotify(text);
    }

    public void ResetSelection()
    {
        SelectedPhrases.Clear();
        CreateLabels();
        UpdateSelectedDisplay();
        StartCoroutine(ResetScrollPosition());
    }
}
