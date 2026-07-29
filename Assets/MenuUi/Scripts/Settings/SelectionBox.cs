using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Altzone.Scripts.ReferenceSheets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectionBox : MonoBehaviour
{
    [SerializeField] private Button _goLeftButton;
    [SerializeField] private Button _goRightButton;
    [SerializeField] private TextMeshProUGUI _text;
    [Space]
    [SerializeField] private SettingsCarrier.SelectionBoxType _type = SettingsCarrier.SelectionBoxType.None;
    public List<string> _manualValues = new List<string>();
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

        if(_type is not SettingsCarrier.SelectionBoxType.None) _manualValues = EnumCategoryToStringList(_type);

        if (_manualValues == null || _manualValues.Count <= 0)
        {
            Debug.LogError($"No SelectionBox values set for {gameObject.name}. ");
            return;
        }

        string savedName = SettingsCarrier.Instance.GetSelectionBoxData(_type);

        if (!string.IsNullOrEmpty(savedName) && _manualValues.Contains(savedName))
        {
            SetText(savedName);
            _index = _manualValues.IndexOf(savedName);
        }
        else
            _text.text = _manualValues[_index];

        _goLeftButton.onClick.AddListener(() => GoToDirection(-1));
        _goRightButton.onClick.AddListener(() => GoToDirection(1));
    }

    private List<string> EnumCategoryToStringList(SettingsCarrier.SelectionBoxType type)
    {
        switch (type)
        {
            case SettingsCarrier.SelectionBoxType.None: return null;
            case SettingsCarrier.SelectionBoxType.MainMenuMusic: return MusicReference.Instance.GetStringList("MainMenu");
        }

        return null;
    }

    private void GoToDirection(int direction)
    {
        _index += direction;

        if (_index >= _manualValues.Count)
            _index = 0;
        else if (_index < 0)
            _index = _manualValues.Count - 1;

        SettingsCarrier.Instance.SetDataFromSelectionBox(_type, _manualValues[_index]);

        _onSelectionChange.Invoke();

        SetText(_manualValues[_index]);

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }

    private void SetText(string text) { _text.text = text; }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SelectionBox))]
public class MyScriptEditor : Editor
{
    SerializedProperty _goLeftButton;
    SerializedProperty _goRightButton;
    SerializedProperty _text;
    SerializedProperty _type;
    SerializedProperty _manualValues;
    SerializedProperty _onSelectionChange;


    private void OnEnable()
    {
        _goLeftButton = serializedObject.FindProperty("_goLeftButton");
        _goRightButton = serializedObject.FindProperty("_goRightButton");
        _text = serializedObject.FindProperty("_text");
        _type = serializedObject.FindProperty("_type");
        _manualValues = serializedObject.FindProperty("_manualValues");
        _onSelectionChange = serializedObject.FindProperty("_onSelectionChange");
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();

        var myScript = target as SelectionBox;

        EditorGUILayout.PropertyField(_goLeftButton);
        EditorGUILayout.PropertyField(_goRightButton);
        EditorGUILayout.PropertyField(_text);
        EditorGUILayout.PropertyField(_type);

        if ((SettingsCarrier.SelectionBoxType)_type.enumValueIndex is SettingsCarrier.SelectionBoxType.None)
        {
            //EditorGUI.indentLevel++;
            //EditorGUILayout.PrefixLabel("Manual Values");
            EditorGUILayout.PropertyField(_manualValues, true);
        }

        EditorGUILayout.PropertyField(_onSelectionChange, true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
