using TMPro;
using UnityEngine;

public class FontSizeCopy : MonoBehaviour
{
    // Confirm button uses Auto font size, this is to make sure cancel button text
    // is the same font size as the confirm button
    [SerializeField] TextMeshProUGUI _confirmButtonText;
    [SerializeField] TextMeshProUGUI _cancelButtonText;

    private void OnEnable()
    {
        _cancelButtonText.fontSize = _confirmButtonText.fontSize;
    }
}
