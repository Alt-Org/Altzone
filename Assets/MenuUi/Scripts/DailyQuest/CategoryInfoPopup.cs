using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryInfoPopup : MonoBehaviour
{
    [SerializeField] private GameObject _categoryInfoWindow;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    public void ShowCategoryInfo(string categoryName)
    {
        List<CategoryInfo> categories = CategoryInfoConfig.Instance.GetCategoryInfos();

        foreach (var category in categories)
        {
            if (category.Name == categoryName)
            {
                _nameText.text = category.Name;
                _nameText.color = category.Color;
                _titleText.text = category.Title;
                _descriptionText.text = category.Description;
            }
        }

    }
}
