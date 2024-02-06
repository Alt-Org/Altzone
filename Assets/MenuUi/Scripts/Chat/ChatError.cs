using TMPro;
using UnityEngine;

public class ChatError : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _errorTextField;

    public void Delete()
    {
        Destroy(gameObject);
    }

    public void SetErrorText(string errorText)
    {
        _errorTextField.text = errorText;
    }
}
