using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SelectionBox : MonoBehaviour
{
    [SerializeField] private Button _goLeftButton;
    [SerializeField] private Button _goRightButton;
    [SerializeField] private TextMeshProUGUI _text;
    [Space]
    [SerializeField] private string _name = "";
    [SerializeField] private List<string> _values = new List<string>();
    [SerializeField] private UnityEvent _onSelectionChange;

    private int _index = 0;

    private void Start()
    {
        if (_goLeftButton == null || _goRightButton == null)
        {
            Debug.LogError("One or both Button's are not assigned! Reported from: " + gameObject.name);
            return;
        }

        if (_text == null)
        {
            Debug.LogError("TextMeshProUGUI is not assigned! Reported from: " + gameObject.name);
            return;
        }

        string savedName = PlayerPrefs.GetString(_name);

        if (!string.IsNullOrEmpty(savedName))
        {
            _text.text = savedName;
            _index = _values.IndexOf(savedName);
        }
        else
            _text.text = _values[_index];

        _goLeftButton.onClick.AddListener(() => GoToDirection(-1));
        _goRightButton.onClick.AddListener(() => GoToDirection(1));
    }

    private void GoToDirection(int direction)
    {
        _index += direction;

        if (_index >= _values.Count)
            _index = 0;
        else if (_index < 0)
            _index = _values.Count - 1;

        PlayerPrefs.SetString(_name, _values[_index]);

        _onSelectionChange.Invoke();

        SetText(_values[_index]);
    }

    private void SetText(string text) { _text.text = text; }
}
