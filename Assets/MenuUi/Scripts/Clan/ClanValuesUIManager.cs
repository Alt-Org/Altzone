using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ClanValueIcon
{
    public ClanValues clanValue;
    public Sprite icon;
}

public class ClanValuesUIManager : MonoBehaviour
{
    [Header("Clan Value Icons")]
    public ClanValueIcon[] clanValueIcons;
    
    [Header("UI Grid")]
    public Transform iconGrid;
    public GameObject iconButtonPrefab; 
    
    [Header("Selected Values")]
    public List<ClanValues> selectedValues = new List<ClanValues>();
    public int maxSelections = 10;
    
    private Dictionary<ClanValues, Sprite> iconDictionary;
    private List<Button> createdButtons = new List<Button>();

    void Start()
    {
        InitializeIconDictionary();
        CreateValueButtons();
    }
    
    // Julkinen metodi UI:n päivittämiseen
    public void RefreshUI()
    {
        InitializeIconDictionary();
        CreateValueButtons();
    }

    void InitializeIconDictionary()
    {
        iconDictionary = new Dictionary<ClanValues, Sprite>();
        
        foreach (var clanIcon in clanValueIcons)
        {
            if (!iconDictionary.ContainsKey(clanIcon.clanValue))
            {
                iconDictionary.Add(clanIcon.clanValue, clanIcon.icon);
            }
        }
    }

    void CreateValueButtons()
    {
        // Tyhjennä aiemmat buttonit
        foreach (Transform child in iconGrid)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        createdButtons.Clear();

        // Luo button jokaiselle ClanValues enumille
        foreach (ClanValues value in System.Enum.GetValues(typeof(ClanValues)))
        {
            GameObject buttonObj = Instantiate(iconButtonPrefab, iconGrid);
            Button button = buttonObj.GetComponent<Button>();
            Image iconImage = buttonObj.GetComponentInChildren<Image>();
            
            // Aseta ikoni jos löytyy
            if (iconDictionary.ContainsKey(value))
            {
                iconImage.sprite = iconDictionary[value];
            }
            
            // Aseta button toiminnallisuus
            ClanValues capturedValue = value; // Lambda capture
            button.onClick.AddListener(() => OnValueSelected(capturedValue, button));
            
            createdButtons.Add(button);
        }
    }

    void OnValueSelected(ClanValues value, Button button)
    {
        if (selectedValues.Contains(value))
        {
            // Poista valinta
            selectedValues.Remove(value);
            UpdateButtonVisual(button, false);
        }
        else if (selectedValues.Count < maxSelections)
        {
            // Lisää valinta
            selectedValues.Add(value);
            UpdateButtonVisual(button, true);
        }
        
        Debug.Log($"Selected values: {string.Join(", ", selectedValues)}");
    }

    void UpdateButtonVisual(Button button, bool isSelected)
    {
        ColorBlock colors = button.colors;
        if (isSelected)
        {
            colors.normalColor = Color.green;
            colors.selectedColor = Color.green;
        }
        else
        {
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
        }
        button.colors = colors;
    }

    // Julkinen metodi valittujen arvojen hakemiseen
    public List<ClanValues> GetSelectedValues()
    {
        return new List<ClanValues>(selectedValues);
    }

    // Metodi asetusten lataamiseen
    public void LoadSelectedValues(List<ClanValues> values)
    {
        selectedValues.Clear();
        selectedValues.AddRange(values.Take(maxSelections));
        
        // Päivitä button visualit
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
        
        if (GUILayout.Button("Auto-Setup Icon Array"))
        {
            SetupIconArray(manager);
        }
        
        if (GUILayout.Button("Refresh UI"))
        {
            if (Application.isPlaying)
            {
                manager.RefreshUI();
            }
        }
    }
    
    void SetupIconArray(ClanValuesUIManager manager)
    {
        var enumValues = System.Enum.GetValues(typeof(ClanValues));
        manager.clanValueIcons = new ClanValueIcon[enumValues.Length];
        
        for (int i = 0; i < enumValues.Length; i++)
        {
            manager.clanValueIcons[i] = new ClanValueIcon
            {
                clanValue = (ClanValues)enumValues.GetValue(i),
                icon = null 
            };
        }
        
        EditorUtility.SetDirty(manager);
    }
}
#endif
