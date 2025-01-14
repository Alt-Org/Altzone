using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopUp : MonoBehaviour
{
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private void Awake()
    {
        _yesButton.onClick.AddListener(Close);
        _noButton.onClick.AddListener(Close);
    }

    private void Close() =>
        gameObject.SetActive(false);
}
