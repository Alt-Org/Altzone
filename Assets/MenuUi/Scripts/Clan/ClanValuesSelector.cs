using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ClanValuesSelector : MonoBehaviour
{
    public Transform iconGrid;
    public GameObject iconPrefab;
    public Button readyButton;
    public int maxSelections = 5;

    private Dictionary<ClanValues, GameObject> iconObjects = new();
    private HashSet<ClanValues> selectedValues = new();

    void Start()
    {
        GenerateIcons();
        readyButton.onClick.AddListener(SaveSelections);
    }

    void GenerateIcons()
    {
        foreach (ClanValues value in System.Enum.GetValues(typeof(ClanValues)))
        {
            GameObject icon = Instantiate(iconPrefab, iconGrid);
            icon.name = value.ToString();

            Sprite sprite = Resources.Load<Sprite>("ClanGraphics/" + value.ToString());
            Image image = icon.transform.Find("IconImage").GetComponent<Image>();
            image.sprite = sprite;

            GameObject highlight = icon.transform.Find("Highlight").gameObject;
            highlight.SetActive(false);

            Button button = icon.GetComponent<Button>();
            button.onClick.AddListener(() => ToggleSelection(value, highlight));
            
            iconObjects.Add(value, icon);
        }
    }

    void ToggleSelection(ClanValues value, GameObject highlight)
    {
        if (selectedValues.Contains(value))
        {
            selectedValues.Remove(value);
            highlight.SetActive(false);
        }
        else if (selectedValues.Count < maxSelections)
        {
            selectedValues.Add(value);
            highlight.SetActive(true);
        }
    }

    void SaveSelections()
    {
        string saved = string.Join(",", selectedValues.Select(v => v.ToString()));
        PlayerPrefs.SetString("SelectedClanValues", saved);
        PlayerPrefs.Save();
        Debug.Log("Valinnat tallennettu: " + saved);
    }

    public static List<ClanValues> LoadSelections()
    {
        string data = PlayerPrefs.GetString("SelectedClanValues", "");
        return data.Split(',')
            .Select(s => System.Enum.TryParse(s, out ClanValues v) ? v : (ClanValues?)null)
            .Where(v => v.HasValue)
            .Select(v => v.Value)
            .ToList();
    }
}
