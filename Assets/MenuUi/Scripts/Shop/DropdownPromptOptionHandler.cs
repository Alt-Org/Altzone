using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownPromptOptionHandler : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TMP_Text _captionLabel;
    [SerializeField] private string _promptText;

    private bool _promptOptionHidden;

    private void Update()
    {
        if (!_dropdown.IsExpanded)
        {
            _promptOptionHidden = false;
            return;
        }

        if (_promptOptionHidden)
            return;

        Transform dropdownList = _dropdown.transform.Find("Dropdown List");
        if (dropdownList == null)
            return;

        Toggle[] items = dropdownList.GetComponentsInChildren<Toggle>(false);
        if (items.Length == 0 || items[0].transform is not RectTransform promptItem)
            return;

        RectTransform content = promptItem.parent as RectTransform;
        float promptItemHeight = promptItem.rect.height;
        promptItem.gameObject.SetActive(false);

        if (content != null)
            content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(0f, content.sizeDelta.y - promptItemHeight));

        _promptOptionHidden = true;
    }

    public void ResetPrompt()
    {
        _promptOptionHidden = false;
        _dropdown.SetValueWithoutNotify(0);
        _dropdown.RefreshShownValue();

        if (_captionLabel != null)
            _captionLabel.text = _promptText;
    }
}
