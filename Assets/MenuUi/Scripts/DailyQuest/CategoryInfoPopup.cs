using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryInfoPopup : MonoBehaviour
{
    [SerializeField] private GameObject _categoryInfoWindow;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private static CategoryInfoPopup _instance;

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public static void ShowCategoryInfo(string categoryId)
    {
        List<CategoryInfo> categories = CategoryInfoConfig.Instance.GetCategoryInfos();

        bool showPopup = false;

        foreach (var category in categories)
        {
            if (category.Id.ToLower() == categoryId.ToLower())
            {
                _instance._nameText.text = category.Name;
                _instance._nameText.color = category.Color;
                _instance._titleText.text = category.Title;
                _instance._descriptionText.text = category.Description;
                showPopup = true;
                break;
            }
        }
        _instance._categoryInfoWindow.SetActive(showPopup);
    }

    public static void HideCategoryInfo()
    {
        _instance._categoryInfoWindow.SetActive(false);
    }
}
