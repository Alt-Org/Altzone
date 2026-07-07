using System;
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
    private const string EmptyCardButtonText = "Kortti";
    private const string MobileButtonText = "Mobiili";
    private const string RequiredMarkerText = "<color=#FF0000>*</color>";
    private const int ExpiryDateDigitCount = 4;
    private const int CvvDigitCount = 3;
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
    private TMP_Text _cardButtonLabel;
    private TMP_Text _mobileButtonLabel;
    private TMP_Text _ratingDropdownLabel;
    private Image _cancelButtonBackground;
    private RectTransform _primaryButtonRectTransform;
    private RectTransform _cancelButtonRectTransform;
    private TMP_Dropdown _ratingDropdown;
    private TMP_Dropdown _savedCardDropdown;
    private TMP_InputField _nameInputField;
    private TMP_InputField _cardInputField;
    private TMP_InputField _dateInputField;
    private TMP_InputField _addCardCvvInputField;
    private TMP_InputField _paymentCvvInputField;
    private TMP_Text _nameInputFieldLabel;
    private TMP_Text _cardInputFieldLabel;
    private TMP_Text _dateInputFieldLabel;
    private TMP_Text _addCardCvvInputFieldLabel;
    private TMP_Text _paymentCvvInputFieldLabel;
    private string _nameInputFieldLabelText;
    private string _cardInputFieldLabelText;
    private string _dateInputFieldLabelText;
    private string _addCardCvvInputFieldLabelText;
    private string _paymentCvvInputFieldLabelText;
    private readonly List<SavedCard> _savedCards = new List<SavedCard>();
    private int _selectedCardIndex = -1;
    private bool _usingCardPayment;
    private bool _ratingPromptOptionHidden;
    private TransactionState _state;
    private Action _onPaymentCompleted;
    private bool _updatingInputFieldText;

    private class SavedCard
    {
        public string HolderName;
        public string LastFourDigits;
        public string DisplayNumber;
        public string ExpiryDate;
    }

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
        BindInputFields();
    }

    private void OnEnable()
    {
        Open();
    }

    private void Update()
    {
        HideRatingPromptOptionWhenExpanded();
    }

    public void Open(Action onPaymentCompleted = null)
    {
        _onPaymentCompleted = onPaymentCompleted;
        CacheReferences();
        BindButtons();
        BindInputFields();
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
        _cardButtonLabel = _cardButton != null ? _cardButton.GetComponentInChildren<TMP_Text>(true) : null;
        _mobileButtonLabel = _mobileButton != null ? _mobileButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cancelButtonBackground = _cancelButton != null ? _cancelButton.GetComponent<Image>() : null;
        _primaryButtonRectTransform = _primaryButton != null ? _primaryButton.GetComponent<RectTransform>() : null;
        _cancelButtonRectTransform = _cancelButton != null ? _cancelButton.GetComponent<RectTransform>() : null;
        _ratingDropdown = FindChildComponent<TMP_Dropdown>("RatingDropdown");
        _ratingDropdownLabel = _ratingDropdown != null ? _ratingDropdown.captionText : null;
        _savedCardDropdown = FindChildComponentIn<TMP_Dropdown>(_selectCardWindow, "Dropdown");
        _nameInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "NameInputField");
        _cardInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "CardInputField");
        _dateInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "DateInputField");
        _addCardCvvInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "CVVInputField");
        _paymentCvvInputField = FindChildComponentIn<TMP_InputField>(_selectCardWindow, "CVVInputField");
        if (_paymentCvvInputField == null)
            _paymentCvvInputField = FindChildComponentIn<TMP_InputField>(_selectCardWindow, "CVVCheckField");

        CacheRequiredFieldLabels();
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

        if (_savedCardDropdown != null)
        {
            _savedCardDropdown.onValueChanged.RemoveListener(SelectSavedCard);

            if (_savedCardDropdown.onValueChanged.GetPersistentEventCount() == 0)
                _savedCardDropdown.onValueChanged.AddListener(SelectSavedCard);
        }
    }

    private void BindInputFields()
    {
        BindInputField(_nameInputField);
        BindNumericInputField(_cardInputField);
        BindDateInputField(_dateInputField);
        BindNumericInputField(_addCardCvvInputField, CvvDigitCount);
        BindNumericInputField(_paymentCvvInputField, CvvDigitCount);
        ClearInputFieldPlaceholder(_addCardCvvInputField);
        ClearInputFieldPlaceholder(_paymentCvvInputField);
    }

    private void BindInputField(TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        inputField.onValueChanged.RemoveListener(InputFieldValueChanged);
        inputField.onValueChanged.AddListener(InputFieldValueChanged);
    }

    private void BindNumericInputField(TMP_InputField inputField, int maxDigits = 0)
    {
        if (inputField == null)
            return;

        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.characterValidation = TMP_InputField.CharacterValidation.Digit;
        inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
        if (maxDigits > 0)
            inputField.characterLimit = maxDigits;
        BindInputField(inputField);
        SanitizeNumericInputField(inputField, maxDigits);
    }

    private void BindDateInputField(TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        inputField.contentType = TMP_InputField.ContentType.Standard;
        inputField.characterValidation = TMP_InputField.CharacterValidation.None;
        inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
        inputField.characterLimit = ExpiryDateDigitCount + 2;
        BindInputField(inputField);
        SanitizeDateInputField(inputField);
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

        RefreshRequiredFieldLabels();
        RefreshPaymentButtons();
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
        RefreshRequiredFieldLabels();

        if (!IsAddCardFormValid())
            return;

        AddCardFromInputs();
        ShowState(TransactionState.PrePayment);
    }

    private void SubmitPayment()
    {
        RefreshRequiredFieldLabels();

        if (!IsPaymentFormValid())
            return;

        ShowState(TransactionState.PostPayment);
        _onPaymentCompleted?.Invoke();
        _onPaymentCompleted = null;
    }

    public void SelectCardPayment()
    {
        if (_savedCards.Count == 0)
        {
            SelectMobilePayment();
            return;
        }

        _usingCardPayment = true;
        if (_mobileButton != null) _mobileButton.interactable = true;
        if (_cardButton != null) _cardButton.interactable = false;
        RefreshRequiredFieldLabels();
    }

    public void SelectMobilePayment()
    {
        _usingCardPayment = false;
        if (_cardButton != null) _cardButton.interactable = _savedCards.Count > 0;
        if (_mobileButton != null) _mobileButton.interactable = false;
        RefreshRequiredFieldLabels();
    }

    public void Close()
    {
        _onPaymentCompleted = null;
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

        _cancelButtonRectTransform.anchorMin = isExitButton && _primaryButtonRectTransform != null ? _primaryButtonRectTransform.anchorMin : new Vector2(0.25f, 0f);
        _cancelButtonRectTransform.anchorMax = isExitButton && _primaryButtonRectTransform != null ? _primaryButtonRectTransform.anchorMax : new Vector2(0.75f, 0.35f);
        _cancelButtonRectTransform.anchoredPosition = Vector2.zero;
        _cancelButtonRectTransform.sizeDelta = Vector2.zero;
    }

    private void ResetRatingDropdown()
    {
        if (_ratingDropdown == null)
            return;

        _ratingPromptOptionHidden = false;
        _ratingDropdown.SetValueWithoutNotify(0);
        _ratingDropdown.RefreshShownValue();
        SetText(_ratingDropdownLabel, RatingPromptText);
    }

    private void HideRatingPromptOptionWhenExpanded()
    {
        if (_ratingDropdown == null)
            return;

        if (!_ratingDropdown.IsExpanded)
        {
            _ratingPromptOptionHidden = false;
            return;
        }

        if (_ratingPromptOptionHidden)
            return;

        Transform dropdownList = _ratingDropdown.transform.Find("Dropdown List");
        if (dropdownList == null)
            return;

        Toggle[] items = dropdownList.GetComponentsInChildren<Toggle>(false);
        if (items.Length == 0)
            return;

        RectTransform promptItem = items[0].transform as RectTransform;
        if (promptItem == null)
            return;

        RectTransform content = promptItem.parent as RectTransform;
        float promptItemHeight = promptItem.rect.height;

        promptItem.gameObject.SetActive(false);

        if (content != null)
            content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(0f, content.sizeDelta.y - promptItemHeight));

        _ratingPromptOptionHidden = true;
    }

    private void AddCardFromInputs()
    {
        string cardNumber = GetInputText(_cardInputField);
        string digits = GetDigits(cardNumber);
        string lastFourDigits = digits.Substring(digits.Length - 4);

        SavedCard card = new SavedCard
        {
            HolderName = GetInputText(_nameInputField),
            LastFourDigits = lastFourDigits,
            DisplayNumber = GetCardDisplayNumber(cardNumber, lastFourDigits),
            ExpiryDate = GetInputText(_dateInputField)
        };

        _savedCards.Add(card);
        _selectedCardIndex = _savedCards.Count - 1;
        _usingCardPayment = true;

        if (_cardInputField != null) _cardInputField.text = string.Empty;
        if (_addCardCvvInputField != null) _addCardCvvInputField.text = string.Empty;
        if (_paymentCvvInputField != null) _paymentCvvInputField.text = string.Empty;

        RefreshPaymentButtons();
    }

    private void CacheRequiredFieldLabels()
    {
        _nameInputFieldLabel = FindTitleText(_nameInputField);
        _cardInputFieldLabel = FindTitleText(_cardInputField);
        _dateInputFieldLabel = FindTitleText(_dateInputField);
        _addCardCvvInputFieldLabel = FindTitleText(_addCardCvvInputField);
        _paymentCvvInputFieldLabel = FindTitleText(_paymentCvvInputField);

        _nameInputFieldLabelText = GetRequiredFieldLabelText(_nameInputFieldLabel, _nameInputFieldLabelText);
        _cardInputFieldLabelText = GetRequiredFieldLabelText(_cardInputFieldLabel, _cardInputFieldLabelText);
        _dateInputFieldLabelText = GetRequiredFieldLabelText(_dateInputFieldLabel, _dateInputFieldLabelText);
        _addCardCvvInputFieldLabelText = GetRequiredFieldLabelText(_addCardCvvInputFieldLabel, _addCardCvvInputFieldLabelText);
        _paymentCvvInputFieldLabelText = GetRequiredFieldLabelText(_paymentCvvInputFieldLabel, _paymentCvvInputFieldLabelText);
    }

    private void RefreshRequiredFieldLabels()
    {
        SetRequiredFieldLabel(_nameInputFieldLabel, _nameInputFieldLabelText, !HasText(_nameInputField));
        SetRequiredFieldLabel(_cardInputFieldLabel, _cardInputFieldLabelText, GetDigits(GetInputText(_cardInputField)).Length < 4);
        SetRequiredFieldLabel(_dateInputFieldLabel, _dateInputFieldLabelText, GetDigits(GetInputText(_dateInputField)).Length < ExpiryDateDigitCount);
        SetRequiredFieldLabel(_addCardCvvInputFieldLabel, _addCardCvvInputFieldLabelText, GetDigits(GetInputText(_addCardCvvInputField)).Length < CvvDigitCount);

        bool paymentCvvRequired = _state == TransactionState.PrePayment && _usingCardPayment;
        SetRequiredFieldLabel(_paymentCvvInputFieldLabel, _paymentCvvInputFieldLabelText, paymentCvvRequired && GetDigits(GetInputText(_paymentCvvInputField)).Length < CvvDigitCount);
    }

    private void InputFieldValueChanged(string value)
    {
        SanitizeNumericInputFields();
        RefreshRequiredFieldLabels();
    }

    private void SanitizeNumericInputFields()
    {
        if (_updatingInputFieldText)
            return;

        SanitizeNumericInputField(_cardInputField);
        SanitizeDateInputField(_dateInputField);
        SanitizeNumericInputField(_addCardCvvInputField, CvvDigitCount);
        SanitizeNumericInputField(_paymentCvvInputField, CvvDigitCount);
    }

    private void SanitizeNumericInputField(TMP_InputField inputField, int maxDigits = 0)
    {
        if (inputField == null)
            return;

        string digits = GetDigits(inputField.text);
        if (maxDigits > 0 && digits.Length > maxDigits)
            digits = digits.Substring(0, maxDigits);

        if (inputField.text == digits)
            return;

        _updatingInputFieldText = true;
        inputField.SetTextWithoutNotify(digits);
        inputField.caretPosition = digits.Length;
        inputField.stringPosition = digits.Length;
        _updatingInputFieldText = false;
    }

    private void SanitizeDateInputField(TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        string formattedDate = GetFormattedExpiryDate(inputField.text);
        if (inputField.text == formattedDate)
            return;

        _updatingInputFieldText = true;
        inputField.SetTextWithoutNotify(formattedDate);
        inputField.caretPosition = formattedDate.Length;
        inputField.stringPosition = formattedDate.Length;
        _updatingInputFieldText = false;
    }

    private void SetRequiredFieldLabel(TMP_Text label, string labelText, bool showRequiredMarker)
    {
        if (label == null)
            return;

        label.text = showRequiredMarker ? labelText + RequiredMarkerText : labelText;
    }

    private string GetRequiredFieldLabelText(TMP_Text label, string fallbackLabelText)
    {
        if (label == null)
            return fallbackLabelText ?? string.Empty;

        string labelText = label.text;
        if (labelText.EndsWith(RequiredMarkerText, StringComparison.Ordinal))
            labelText = labelText.Substring(0, labelText.Length - RequiredMarkerText.Length);

        return labelText;
    }

    private bool IsAddCardFormValid()
    {
        if (!HasText(_nameInputField))
            return FocusInputField(_nameInputField);

        if (GetDigits(GetInputText(_cardInputField)).Length < 4)
            return FocusInputField(_cardInputField);

        if (GetDigits(GetInputText(_dateInputField)).Length < ExpiryDateDigitCount)
            return FocusInputField(_dateInputField);

        if (GetDigits(GetInputText(_addCardCvvInputField)).Length < CvvDigitCount)
            return FocusInputField(_addCardCvvInputField);

        return true;
    }

    private bool IsPaymentFormValid()
    {
        if (!_usingCardPayment)
            return true;

        if (_selectedCardIndex < 0 || _selectedCardIndex >= _savedCards.Count)
            return false;

        if (GetDigits(GetInputText(_paymentCvvInputField)).Length < CvvDigitCount)
            return FocusInputField(_paymentCvvInputField);

        return true;
    }

    private void RefreshPaymentButtons()
    {
        SetText(_cardButtonLabel, EmptyCardButtonText);
        SetText(_mobileButtonLabel, MobileButtonText);
        RefreshSavedCardDropdown();

        if (_usingCardPayment && _savedCards.Count > 0)
            SelectCardPayment();
        else
            SelectMobilePayment();
    }

    private void RefreshSavedCardDropdown()
    {
        if (_savedCardDropdown == null)
            return;

        _savedCardDropdown.onValueChanged.RemoveListener(SelectSavedCard);
        _savedCardDropdown.ClearOptions();

        List<string> cardOptions = new List<string>();
        for (int i = 0; i < _savedCards.Count; i++)
            cardOptions.Add(GetSavedCardText(_savedCards[i]));

        _savedCardDropdown.AddOptions(cardOptions);
        _savedCardDropdown.interactable = _savedCards.Count > 0;

        if (_savedCards.Count > 0)
        {
            _selectedCardIndex = Mathf.Clamp(_selectedCardIndex, 0, _savedCards.Count - 1);
            _savedCardDropdown.value = _selectedCardIndex;
        }
        else
        {
            _selectedCardIndex = -1;
        }

        _savedCardDropdown.RefreshShownValue();
        _savedCardDropdown.onValueChanged.AddListener(SelectSavedCard);
    }

    private void SelectSavedCard(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= _savedCards.Count)
            return;

        _selectedCardIndex = selectedIndex;
        _usingCardPayment = true;
        SelectCardPayment();
    }

    private string GetSavedCardText(SavedCard card)
    {
        return "VISA  " + card.DisplayNumber;
    }

    private string GetCardDisplayNumber(string cardNumber, string lastFourDigits)
    {
        if (!string.IsNullOrWhiteSpace(cardNumber))
            return cardNumber.Trim();

        return "**** " + lastFourDigits;
    }

    private string GetInputText(TMP_InputField inputField)
    {
        return inputField != null ? inputField.text.Trim() : string.Empty;
    }

    private string GetFormattedExpiryDate(string value)
    {
        string digits = GetDigits(value);
        if (digits.Length > ExpiryDateDigitCount)
            digits = digits.Substring(0, ExpiryDateDigitCount);

        if (digits.Length < 2)
            return digits;

        if (digits.Length == 2)
            return digits + "/ ";

        return digits.Substring(0, 2) + "/ " + digits.Substring(2);
    }

    private bool HasText(TMP_InputField inputField)
    {
        return !string.IsNullOrWhiteSpace(GetInputText(inputField));
    }

    private bool HasDigits(TMP_InputField inputField)
    {
        return GetDigits(GetInputText(inputField)).Length > 0;
    }

    private bool FocusInputField(TMP_InputField inputField)
    {
        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }

        return false;
    }

    private string GetDigits(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        string digits = string.Empty;
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsDigit(value[i]))
                digits += value[i];
        }

        return digits;
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

    private T FindChildComponentIn<T>(GameObject parent, string childName) where T : Component
    {
        if (parent == null)
            return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child.GetComponent<T>();
        }

        return null;
    }

    private TMP_Text FindTitleText(TMP_InputField inputField)
    {
        if (inputField == null)
            return null;

        Transform titleTransform = inputField.transform.Find("TitleText");
        return titleTransform != null ? titleTransform.GetComponent<TMP_Text>() : null;
    }

    private void ClearInputFieldPlaceholder(TMP_InputField inputField)
    {
        if (inputField == null || inputField.placeholder == null)
            return;

        TMP_Text placeholderText = inputField.placeholder.GetComponent<TMP_Text>();
        if (placeholderText != null)
            placeholderText.text = string.Empty;
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
