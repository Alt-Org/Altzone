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
        _grid.padding = new RectOffset(0, 0, 10, 0);
        _grid.cellSize = new Vector2(400, 65);
        _grid.spacing = new Vector2(10, 10);
        _grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        _grid.constraintCount = 3;

        _values = clanValues ?? new List<ClanValues>();

        foreach (Transform child in _grid.transform) Destroy(child.gameObject);

        int count = Mathf.Min(3, _values.Count);

        for (int i = 0; i < count; i++)
        {
            var value = _values[i];
            GameObject labelPanel = Instantiate(_valuePrefab, _grid.transform);
            ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();
            labelHandler.SetLabelInfo(_values[i], true);
        }
        //foreach (ClanValues value in _values)
        //{
        //    GameObject labelPanel = Instantiate(_valuePrefab, _grid.transform);
        //    ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();
        //    labelHandler.SetLabelInfo(value, true);
        //}
    }
}
