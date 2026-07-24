using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarShopCategoryUI : MonoBehaviour
{
    [System.Serializable]
    private class CategoryGroup
    {
        public AvatarPiece AvatarPiece;
        public GameObject Group;
    }

    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private AvatarShopStorage avatarShopStorage;

    [Header("Entire category containers")]
    [SerializeField] private List<CategoryGroup> categoryGroups;

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

        foreach (CategoryGroup categoryGroup in categoryGroups)
        {
            Transform titleTransform = categoryGroup.Group.transform.GetChild(0);

            TMP_Text titleText = titleTransform.GetComponentInChildren<TMP_Text>();

            options.Add(titleText.text);
        }

        categoryDropdown.AddOptions(options);
    }

    private void OnCategoryChanged(int index)
    {
        if (index < 0 || index >= categoryGroups.Count)
        {
            return;
        }

        AvatarPiece selectedPiece = categoryGroups[index].AvatarPiece;

        foreach (CategoryGroup categoryGroup in categoryGroups)
        {
            categoryGroup.Group.SetActive(categoryGroup.AvatarPiece == selectedPiece);
        }

        RefreshLayout(selectedPiece);
    }

    private void RefreshLayout(AvatarPiece avatarPiece)
    {
        Canvas.ForceUpdateCanvases();

        CategoryGroup selectedCategory = categoryGroups.Find(categoryGroup => categoryGroup.AvatarPiece == avatarPiece);

        if (selectedCategory?.Group != null && selectedCategory.Group.transform is RectTransform selectedGroup)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectedGroup);
        }

        BaseScrollRect shopScrollRect = avatarShopStorage.ScrollRect;

        if (shopScrollRect != null)
        {
            shopScrollRect.StopMovement();

            if (shopScrollRect.Content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollRect.Content);
            }

            shopScrollRect.VerticalNormalizedPosition = 1f;
        }

        Canvas.ForceUpdateCanvases();
    }
}
