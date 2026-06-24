using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayTransactionPopUpHandler : MonoBehaviour
{
    private const string AddCardButtonText = "Lis\u00E4\u00E4 kortti";
    private const string PayButtonText = "Maksa";
    private const string CancelButtonText = "Peruuta";
    private const string ExitButtonText = "Poistu";
    private const string RatingPromptText = "Miksi t\u00E4m\u00E4 on arvokas sinulle?";
    private static readonly List<string> RatingOptions = new()
    {
        "\u2022 Tykk\u00E4\u00E4n tuotteen ulkoasusta",
        "\u2022 Ostos on edullinen",
        "\u2022 Hinnalla ei ole v\u00E4li\u00E4",
        "\u2022 Klaanin yhteinen p\u00E4\u00E4t\u00F6s",
        "\u2022 Haluan erottua paremmin"
    };
    private static readonly Color CancelButtonTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private static readonly Color ExitButtonColor = new Color(0.118f, 0.165f, 0.878f, 1f);

    private GameObject _panel;
    private GameObject _addCardWindow;
    private GameObject _prePaymentWindow;
    private GameObject _selectCardWindow;
    private GameObject _postPaymentWindow;

    private Button _primaryButton;
    private Button _cancelButton;
    private Button _cardButton;
    private Button _mobileButton;

    private TMP_Text _primaryButtonLabel;
    private TMP_Text _cancelButtonLabel;
    private TMP_Text _ratingDropdownLabel;
    private Image _cancelButtonBackground;
    private RectTransform _cancelButtonRectTransform;
    private TMP_Dropdown _ratingDropdown;
    private TMP_InputField _cardInputField;
    private TMP_InputField _cvvInputField;
    private TransactionState _state;

    private enum TransactionState
    {
        AddCard,
        PrePayment,
        PostPayment
    }

    private void Awake()
    {
        CacheReferences();
        BindButtons();
    }

    private void OnEnable()
    {
        Open();
    }

    public void Open()
    {
        CacheReferences();
        BindButtons();
        ShowState(TransactionState.AddCard);
    }

    private void CacheReferences()
    {
        _panel = FindChildGameObject("Panel");
        _addCardWindow = FindChildGameObject("AddCardWindow");
        _prePaymentWindow = FindChildGameObject("PrePaymentWIndow");
        _selectCardWindow = FindChildGameObject("SelectCardWindow");
        _postPaymentWindow = FindChildGameObject("PostPaymentWindow");

        _primaryButton = FindChildComponent<Button>("AddCardButton");
        _cancelButton = FindChildComponent<Button>("CancelButton");
        _cardButton = FindChildComponent<Button>("CardButtonPlaceholder");
        _mobileButton = FindChildComponent<Button>("MobileButtonPlaceholder");

        _primaryButtonLabel = _primaryButton != null ? _primaryButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cancelButtonLabel = _cancelButton != null ? _cancelButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cancelButtonBackground = _cancelButton != null ? _cancelButton.GetComponent<Image>() : null;
        _cancelButtonRectTransform = _cancelButton != null ? _cancelButton.GetComponent<RectTransform>() : null;
        _ratingDropdown = FindChildComponent<TMP_Dropdown>("RatingDropdown");
        _ratingDropdownLabel = _ratingDropdown != null ? _ratingDropdown.captionText : null;
        _cardInputField = FindChildComponent<TMP_InputField>("CardInputField");
        _cvvInputField = FindChildComponent<TMP_InputField>("CVVInputField");
    }

    private void BindButtons()
    {
        if (_primaryButton != null)
        {
            _primaryButton.onClick.RemoveListener(PrimaryButtonClicked);

            if (_primaryButton.onClick.GetPersistentEventCount() == 0)
                _primaryButton.onClick.AddListener(PrimaryButtonClicked);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(Close);

            if (_cancelButton.onClick.GetPersistentEventCount() == 0)
                _cancelButton.onClick.AddListener(Close);
        }

        if (_cardButton != null)
        {
            _cardButton.onClick.RemoveListener(SelectCardPayment);

            if (_cardButton.onClick.GetPersistentEventCount() == 0)
                _cardButton.onClick.AddListener(SelectCardPayment);
        }

        if (_mobileButton != null)
        {
            _mobileButton.onClick.RemoveListener(SelectMobilePayment);

            if (_mobileButton.onClick.GetPersistentEventCount() == 0)
                _mobileButton.onClick.AddListener(SelectMobilePayment);
        }
    }

    private void ShowState(TransactionState state)
    {
        _state = state;

        if (_panel != null) _panel.SetActive(true);
        if (_addCardWindow != null) _addCardWindow.SetActive(state == TransactionState.AddCard);
        if (_prePaymentWindow != null) _prePaymentWindow.SetActive(state == TransactionState.AddCard || state == TransactionState.PrePayment);
        if (_selectCardWindow != null) _selectCardWindow.SetActive(state == TransactionState.PrePayment);
        if (_postPaymentWindow != null) _postPaymentWindow.SetActive(state == TransactionState.PostPayment);

        if (_primaryButton != null)
        {
            bool showPrimaryButton = state != TransactionState.PostPayment;
            _primaryButton.gameObject.SetActive(showPrimaryButton);

            if (state == TransactionState.AddCard)
            {
                SetText(_primaryButtonLabel, AddCardButtonText);
            }
            else if (state == TransactionState.PrePayment)
            {
                SetText(_primaryButtonLabel, PayButtonText);
            }
        }

        SetText(_cancelButtonLabel, state == TransactionState.PostPayment ? ExitButtonText : CancelButtonText);
        SetCancelButtonStyle(state == TransactionState.PostPayment);
        if (state == TransactionState.PostPayment)
            ResetRatingDropdown();

        SelectCardPayment();
    }

    public void PrimaryButtonClicked()
    {
        if (_state == TransactionState.AddCard)
        {
            SubmitCard();
            return;
        }

        if (_state == TransactionState.PrePayment)
            SubmitPayment();
    }

    private void SubmitCard()
    {
        ShowState(TransactionState.PrePayment);
    }

    private void SubmitPayment()
    {
        ShowState(TransactionState.PostPayment);
    }

    public void SelectCardPayment()
    {
        if (_cardButton != null) _cardButton.interactable = false;
        if (_mobileButton != null) _mobileButton.interactable = true;
    }

    public void SelectMobilePayment()
    {
        if (_cardButton != null) _cardButton.interactable = true;
        if (_mobileButton != null) _mobileButton.interactable = false;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null) text.text = value;
    }

    private void SetCancelButtonStyle(bool isExitButton)
    {
        if (_cancelButtonBackground != null)
            _cancelButtonBackground.color = isExitButton ? ExitButtonColor : Color.white;

        if (_cancelButtonLabel != null)
            _cancelButtonLabel.color = isExitButton ? Color.white : CancelButtonTextColor;

        if (_cancelButtonRectTransform == null)
            return;

        _cancelButtonRectTransform.anchorMin = isExitButton ? new Vector2(0.08f, 0.05f) : new Vector2(0.25f, 0f);
        _cancelButtonRectTransform.anchorMax = isExitButton ? new Vector2(0.92f, 0.45f) : new Vector2(0.75f, 0.35f);
        _cancelButtonRectTransform.anchoredPosition = Vector2.zero;
        _cancelButtonRectTransform.sizeDelta = Vector2.zero;
    }

    private void ResetRatingDropdown()
    {
        if (_ratingDropdown == null)
            return;

        _ratingDropdown.ClearOptions();
        _ratingDropdown.AddOptions(RatingOptions);
        _ratingDropdown.value = 0;
        _ratingDropdown.RefreshShownValue();
        SetText(_ratingDropdownLabel, RatingPromptText);
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform child = FindChild(childName);
        return child != null ? child.gameObject : null;
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform child = FindChild(childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private Transform FindChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
