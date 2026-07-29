using TMPro;
using UnityEngine;

public class DropdownPromptOptionHandler : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TMP_Text _captionLabel;
    [SerializeField] private string _promptText;

    public void ResetPrompt()
    {
        _dropdown.SetValueWithoutNotify(0);
        _dropdown.RefreshShownValue();

        if (_captionLabel != null)
            _captionLabel.text = _promptText;
    }
}
