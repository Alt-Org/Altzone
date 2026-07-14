using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryInfoPopup : MonoBehaviour
{
    [SerializeField] private GameObject _categoryInfoWindow;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private static CategoryInfoPopup instance;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }


    public static void ShowCategoryInfo(string categoryId)
    {
        
        List<CategoryInfo> categories = CategoryInfoConfig.Instance.GetCategoryInfos();

        bool showPopup = false;

        foreach (var category in categories)
        {
            if (category.Id.ToLower() == categoryId.ToLower())
            {
                instance._nameText.text = category.Name;
                instance._nameText.color = category.Color;
                instance._titleText.text = category.Title;
                instance._descriptionText.text = category.Description;
                showPopup = true;
                break;
            }
        }
        instance._categoryInfoWindow.SetActive(showPopup);
    }

    public static void HideCategoryInfo()
    {
        instance._categoryInfoWindow.SetActive(false);
    }
}
