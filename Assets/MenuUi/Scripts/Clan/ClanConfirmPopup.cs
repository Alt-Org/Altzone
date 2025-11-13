using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ClanConfirmPopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private TMP_Text _confirmLabel;
    [SerializeField] private TMP_Text _cancelLabel;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _cancelBtn;

    private void Awake()
    {
        if (_canvasGroup)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public void Show(string bodyText, UnityAction onConfirm, UnityAction onCancel = null,
                     string confirmText = "OK", string cancelText = "Peruuta",
                     string style = "default")
    {
        gameObject.SetActive(true);

        if (_confirmBtn) _confirmBtn.interactable = true;
        if (_cancelBtn) _cancelBtn.interactable = true;

        if (_body)
        {
            _body.text = bodyText;
        }

        if (_confirmLabel)
        {
            _confirmLabel.text = confirmText;
        }

        if (_cancelLabel)
        {
            _cancelLabel.text = cancelText;
        }

        if (_confirmBtn)
        {
            _confirmBtn.onClick.RemoveAllListeners();
            _confirmBtn.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                Hide();
            });
        }

        if (_cancelBtn)
        {
            _cancelBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.AddListener(() =>
            {
                onCancel?.Invoke();
                Hide();
            });
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (_canvasGroup)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }
}
