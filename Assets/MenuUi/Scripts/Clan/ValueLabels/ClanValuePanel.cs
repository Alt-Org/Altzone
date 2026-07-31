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
        _values = clanValues ?? new List<ClanValues>();

        foreach (Transform child in _grid.transform)
        {
            Destroy(child.gameObject);
        }

        int count = Mathf.Min(3, _values.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject labelPanel = Instantiate(_valuePrefab, _grid.transform);

            ValueLabelHandler labelHandler = labelPanel.GetComponent<ValueLabelHandler>();

            if (labelHandler != null)
            {
                labelHandler.SetLabelInfo(_values[i], true);
            }
        }
    }
}
