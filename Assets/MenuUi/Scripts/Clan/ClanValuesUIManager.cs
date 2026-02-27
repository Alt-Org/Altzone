using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Clan;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClanValuesUIManager : MonoBehaviour
{
    [Header("Label Reference (ScriptableObject)")]
    public LabelReference labelReference;

    [Header("UI Grid")]
    public Transform iconGrid;
    public GameObject iconButtonPrefab;

    [Header("Selected Values")]
    public List<ClanValues> selectedValues = new List<ClanValues>();
    public int maxSelections = 10;

    [Header("Selection Colors (Hex)")]
    [SerializeField] private string selectedColorHex = "#00FF00";   // Vihreä
    [SerializeField] private string unselectedColorHex = "#CBCBCB"; // Valkoinen

    private Dictionary<ClanValues, Sprite> iconDictionary;
    private List<IconButtonHandler> createdButtons = new List<IconButtonHandler>();

    void Start()
    {
        InitializeIconDictionary();
        CreateValueButtons();
    }

    public void RefreshUI()
    {
        InitializeIconDictionary();
        CreateValueButtons();
    }

    void InitializeIconDictionary()
    {
        iconDictionary = new Dictionary<ClanValues, Sprite>();

        foreach (ClanValues value in System.Enum.GetValues(typeof(ClanValues)))
        {
            Sprite sprite = labelReference.GetLabelImage(value);
            if (sprite != null)
            {
                iconDictionary[value] = sprite;
            }
        }
    }

    void CreateValueButtons()
    {
        foreach (Transform child in iconGrid)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        createdButtons.Clear();

        foreach (ClanValues value in System.Enum.GetValues(typeof(ClanValues)))
        {
            GameObject buttonObj = Instantiate(iconButtonPrefab, iconGrid);
            IconButtonHandler handler = buttonObj.GetComponent<IconButtonHandler>();

            iconDictionary.TryGetValue(value, out Sprite sprite);

            // Use enum name as label. Replace with labelReference lookup if you have a method for text labels.
            string label = value.ToString();

            handler.Initialize(
                sprite,
                value,
                label,
                selectedColorHex,
                unselectedColorHex,
                (ActionValue, callback) => OnValueSelected(ActionValue, callback));

            createdButtons.Add(handler);
        }
    }

    void OnValueSelected(ClanValues value, Action<bool> callback)
    {
        bool active = false;
        if (selectedValues.Contains(value))
        {
            selectedValues.Remove(value);
        }
        else if (selectedValues.Count < maxSelections)
        {
            selectedValues.Add(value);
            active = true;
        }

        Debug.Log($"Selected values: {string.Join(", ", selectedValues)}");
        callback(active);
    }

    public List<ClanValues> GetSelectedValues()
    {
        return new List<ClanValues>(selectedValues);
    }

    public void LoadSelectedValues(List<ClanValues> values)
    {
        selectedValues.Clear();
        selectedValues.AddRange(values.Take(maxSelections));

        for (int i = 0; i < createdButtons.Count; i++)
        {
            var buttonValue = (ClanValues)i;
            bool isSelected = selectedValues.Contains(buttonValue);
            createdButtons[i].UpdateButtonVisual(isSelected);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ClanValuesUIManager))]
public class ClanValuesUIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClanValuesUIManager manager = (ClanValuesUIManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh UI"))
        {
            if (Application.isPlaying)
            {
                manager.RefreshUI();
            }
        }
    }
}
#endif

