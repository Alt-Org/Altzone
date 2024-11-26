using System;
using System.Collections.Generic;
using UnityEngine;

public enum ClanValues
{
    Elainrakkaat,
    Maahanmuuttomyonteiset,
    Lgbtq,
    Raittiit,
    Kohteliaat,
    Kiusaamisenvastaiset,
    Urheilevat,
    Syvalliset,
    Oikeudenmukaiset,
    Kaikkienkaverit,
    Itsenaiset,
    Retkeilijat,
    Suomenruotsalaiset,
    Huumorintajuiset,
    Rikkaat,
    Ikiteinit,
    Juoruilevat,
    Rakastavat,
    Oleilijat,
    Nortit,
    Musadiggarit,
    Tunteelliset,
    Gamerit,
    Animefanit,
    Sinkut,
    Monikulttuuriset,
    Kauniit,
    Jarjestelmalliset,
    Epajarjestelmalliset,
    Tasaarvoiset,
    Somepersoonat,
    Kadentaitajat,
    Muusikot,
    Taiteilijat,
    Spammaajat,
    Kasvissyojat,
    Tasapainoiset,
}

public class ValuePanelController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _labelTogglePrefab;
    [SerializeField] private GameObject _labelImagePrefab;

    [Header("Containers")]
    [SerializeField] private Transform _valueListParent;
    [SerializeField] private Transform _selectedValuesParent;

    private List<ValueLabelHandler> _labelHandlers = new();
    public List<ClanValues> SelectedValues { get; private set; } = new();

    void Start() => CreateLabels();

    public void CreateLabels()
    {
        _labelHandlers.Clear();

        foreach (Transform child in _valueListParent) Destroy(child.gameObject);

        foreach (ClanValues value in Enum.GetValues(typeof(ClanValues)))
        {
            GameObject labelPanel = Instantiate(_labelTogglePrefab, _valueListParent);
            ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();
            labelHandler.SetLabelInfo(value);
            _labelHandlers.Add(labelHandler);

            labelHandler._selectButton.onClick.AddListener(() => ToggleValue(labelHandler));
        }
    }

    public void ToggleValue(ValueLabelHandler toggledHandler)
    {
        if (SelectedValues.Contains(toggledHandler.labelInfo.values))
        {
            ValueLabelHandler handlerOfRemoved = _labelHandlers.Find(handler => handler.labelInfo.values == toggledHandler.labelInfo.values);
            handlerOfRemoved.Unselect();
            SelectedValues.Remove(toggledHandler.labelInfo.values);
        }
        else
        {
            if (SelectedValues.Count >= 5)
            {
                ClanValues removedValue = SelectedValues[0];
                ValueLabelHandler handlerOfRemoved = _labelHandlers.Find(labelHandler => labelHandler.labelInfo.values == removedValue);
                handlerOfRemoved.Unselect();
                SelectedValues.RemoveAt(0);
            }

            SelectedValues.Add(toggledHandler.labelInfo.values);
            ValueLabelHandler handlerOfSelected = _labelHandlers.Find(handler => handler.labelInfo.values == toggledHandler.labelInfo.values);
            handlerOfSelected.Select();
        }

        UpdateSelectedDisplay();
    }

    private void UpdateSelectedDisplay()
    {
        foreach (Transform child in _selectedValuesParent) Destroy(child.gameObject);

        foreach (ClanValues selectedValue in SelectedValues)
        {
            GameObject label = Instantiate(_labelImagePrefab, _selectedValuesParent);
            ValueImageHandle imageHandler = label.GetComponent<ValueImageHandle>();
            imageHandler.SetLabelInfo(selectedValue);
        }
    }
}