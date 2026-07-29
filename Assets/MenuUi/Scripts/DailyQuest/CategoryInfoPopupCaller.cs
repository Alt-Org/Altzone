using UnityEngine;

/// <summary>
/// This class is used for buttons, that should call for the category info popup
/// </summary>
public class CategoryInfoPopupCaller : MonoBehaviour
{
    public void CallForPopup(string categoryId)
    {
        if (categoryId == null) return;

        CategoryInfoPopup.ShowCategoryInfo(categoryId);

    }

    public void HidePopup()
    {
        CategoryInfoPopup.HideCategoryInfo();
    }
}
