using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvatarShopCategoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown categoryDropdown;

    [Header("Entire category containers")]
    [SerializeField] private List<GameObject> categoryGroups;

    private void Start()
    {
        PopulateDropdown();

        categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);

        OnCategoryChanged(0);
    }

    private void PopulateDropdown()
    {
        categoryDropdown.ClearOptions();

        List<string> options = new();

        foreach (GameObject group in categoryGroups)
        {
            options.Add(group.name);
        }

        categoryDropdown.AddOptions(options);
    }

    private void OnCategoryChanged(int index)
    {
        for (int i = 0; i < categoryGroups.Count; i++)
        {
            categoryGroups[i].SetActive(i == index);
        }
    }
}
