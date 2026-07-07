using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarShopCategoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private BaseScrollRect shopScrollRect;

    [Header("Entire category containers")]
    [SerializeField] private List<GameObject> categoryGroups;

    private Coroutine refreshCoroutine;

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
            Transform titleTransform = group.transform.GetChild(0);

            TMP_Text titleText = titleTransform.GetComponentInChildren<TMP_Text>();

            options.Add(titleText.text);
        }

        categoryDropdown.AddOptions(options);
    }

    private void OnCategoryChanged(int index)
    {
        for (int i = 0; i < categoryGroups.Count; i++)
        {
            categoryGroups[i].SetActive(i == index);
        }

        RefreshLayout(index);

        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }

        refreshCoroutine = StartCoroutine(RefreshLayoutNextFrame(index));
    }

    private IEnumerator RefreshLayoutNextFrame(int index)
    {
        yield return null;
        RefreshLayout(index);
        refreshCoroutine = null;
    }

    private void RefreshLayout(int index)
    {
        if (shopScrollRect == null)
        {
            shopScrollRect = GetComponentInParent<BaseScrollRect>();
        }

        Canvas.ForceUpdateCanvases();

        if (index >= 0 && index < categoryGroups.Count && categoryGroups[index] != null
            && categoryGroups[index].transform is RectTransform selectedGroup)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(selectedGroup);
        }

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
