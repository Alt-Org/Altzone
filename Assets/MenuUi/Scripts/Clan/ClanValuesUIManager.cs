using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Clan;

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
    [SerializeField] private string unselectedColorHex = "#FFFFFF"; // Valkoinen

    private Dictionary<ClanValues, Sprite> iconDictionary;
    private List<Button> createdButtons = new List<Button>();

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
            Button button = buttonObj.GetComponent<Button>();
            Image iconImage = buttonObj.GetComponentInChildren<Image>();

            if (iconDictionary.TryGetValue(value, out Sprite sprite))
            {
                iconImage.sprite = sprite;
            }

            ClanValues capturedValue = value;
            button.onClick.AddListener(() => OnValueSelected(capturedValue, button));

            createdButtons.Add(button);
        }
    }

    void OnValueSelected(ClanValues value, Button button)
    {
        if (selectedValues.Contains(value))
        {
            selectedValues.Remove(value);
            UpdateButtonVisual(button, false);
        }
        else if (selectedValues.Count < maxSelections)
        {
            selectedValues.Add(value);
            UpdateButtonVisual(button, true);
        }

        Debug.Log($"Selected values: {string.Join(", ", selectedValues)}");
    }

    void UpdateButtonVisual(Button button, bool isSelected)
    {
        Image targetGraphic = button.targetGraphic as Image;

        if (targetGraphic != null)
        {
            string hexColor = isSelected ? selectedColorHex : unselectedColorHex;

            if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
            {
                targetGraphic.color = parsedColor;
            }
            else
            {
                Debug.LogWarning($"Virheellinen väriarvo: {hexColor}");
            }
        }
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
            UpdateButtonVisual(createdButtons[i], isSelected);
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

