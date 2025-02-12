using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanValuePanel : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup _grid;
    [SerializeField] private GameObject _valuePrefab;
    private List<ClanValues> _values;

    public void SetValues(List<ClanValues> clanValues)
    {
        _values = clanValues;

        foreach (Transform child in _grid.transform) Destroy(child.gameObject);

        foreach (ClanValues value in _values)
        {
            GameObject labelPanel = Instantiate(_valuePrefab, _grid.transform);
            ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();
            labelHandler.SetLabelInfo(value, true);
        }
    }
}
