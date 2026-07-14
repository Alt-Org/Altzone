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
    private const string AddAnotherCardText = "+ Lis\u00E4\u00E4 kortti";
    private const string RequiredMarkerText = "<color=#FF0000>*</color>";
    private const int ExpiryDateDigitCount = 4;
    private const int CvvDigitCount = 3;
    private static readonly Color CancelButtonTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private static readonly Color ExitButtonColor = new Color(0.118f, 0.165f, 0.878f, 1f);
    private static readonly Color CardRowTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private static readonly Color RememberCardEnabledColor = new Color(0.118f, 0.165f, 0.878f, 1f);
    private static readonly Color RememberCardDisabledColor = new Color(0.75f, 0.78f, 0.84f, 1f);

    private GameObject _panel;
    private GameObject _addCardWindow;
    private GameObject _prePaymentWindow;
    private GameObject _selectCardWindow;
    private GameObject _postPaymentWindow;

    private Button _primaryButton;
    private Button _cancelButton;
    private Button _cardButton;
    private Button _mobileButton;
    private Button _rememberCardButton;

    private TMP_Text _primaryButtonLabel;
    private TMP_Text _cancelButtonLabel;
    private TMP_Text _cardButtonLabel;
    private TMP_Text _mobileButtonLabel;
    private TMP_Text _ratingDropdownLabel;
    private Image _cancelButtonBackground;
    private Image _rememberCardBackground;
    private RectTransform _primaryButtonRectTransform;
    private RectTransform _cancelButtonRectTransform;
    private TMP_Dropdown _ratingDropdown;
    private TMP_Dropdown _savedCardListSource;
    private RectTransform _savedCardListRoot;
    private TMP_Text _savedCardListTitle;
    private GameObject _selectedCardRow;
    private GameObject _savedCardRowTemplate;
    private GameObject _addCardRow;
    private Button _selectedCardRowButton;
    private Button _addCardRowButton;
    private TMP_Text _selectedCardRowLabel;
    private TMP_Text _savedCardRowTemplateLabel;
    private TMP_Text _addCardRowLabel;
    private TMP_InputField _nameInputField;
    private TMP_InputField _cardInputField;
    private TMP_InputField _dateInputField;
    private TMP_InputField _addCardCvvInputField;
    private TMP_Text _nameInputFieldLabel;
    private TMP_Text _cardInputFieldLabel;
    private TMP_Text _dateInputFieldLabel;
    private TMP_Text _addCardCvvInputFieldLabel;
    private string _nameInputFieldLabelText;
    private string _cardInputFieldLabelText;
    private string _dateInputFieldLabelText;
    private string _addCardCvvInputFieldLabelText;
    private readonly List<SavedCard> _savedCards = new List<SavedCard>();
    private readonly List<GameObject> _savedCardRows = new List<GameObject>();
    private int _selectedCardIndex = -1;
    private bool _usingCardPayment;
    private bool _rememberCard = true;
    private bool _savedCardListExpanded;
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
        public bool Remembered;
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
        RemoveTemporaryCards();
        _savedCardListExpanded = false;
        ShowState(_savedCards.Count > 0 ? TransactionState.PrePayment : TransactionState.AddCard);
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
        _rememberCardButton = FindChildComponentIn<Button>(_addCardWindow, "RememberCardToggle");

        _primaryButtonLabel = _primaryButton != null ? _primaryButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cancelButtonLabel = _cancelButton != null ? _cancelButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cardButtonLabel = _cardButton != null ? _cardButton.GetComponentInChildren<TMP_Text>(true) : null;
        _mobileButtonLabel = _mobileButton != null ? _mobileButton.GetComponentInChildren<TMP_Text>(true) : null;
        _cancelButtonBackground = _cancelButton != null ? _cancelButton.GetComponent<Image>() : null;
        _rememberCardBackground = _rememberCardButton != null ? _rememberCardButton.GetComponent<Image>() : null;
        _primaryButtonRectTransform = _primaryButton != null ? _primaryButton.GetComponent<RectTransform>() : null;
        _cancelButtonRectTransform = _cancelButton != null ? _cancelButton.GetComponent<RectTransform>() : null;
        _ratingDropdown = FindChildComponent<TMP_Dropdown>("RatingDropdown");
        _ratingDropdownLabel = _ratingDropdown != null ? _ratingDropdown.captionText : null;
        _savedCardListSource = FindChildComponentIn<TMP_Dropdown>(_selectCardWindow, "SavedCardList");
        _savedCardListRoot = _savedCardListSource != null ? _savedCardListSource.transform as RectTransform : null;
        Transform savedCardTitleTransform = _savedCardListSource != null ? _savedCardListSource.transform.Find("TitleText") : null;
        _savedCardListTitle = savedCardTitleTransform != null ? savedCardTitleTransform.GetComponent<TMP_Text>() : null;
        _selectedCardRow = FindChildGameObjectIn(_savedCardListSource != null ? _savedCardListSource.gameObject : null, "SelectedCardRow");
        _savedCardRowTemplate = FindChildGameObjectIn(_savedCardListSource != null ? _savedCardListSource.gameObject : null, "SavedCardRowTemplate");
        _addCardRow = FindChildGameObjectIn(_savedCardListSource != null ? _savedCardListSource.gameObject : null, "AddCardRow");
        _selectedCardRowButton = _selectedCardRow != null ? _selectedCardRow.GetComponent<Button>() : null;
        _addCardRowButton = _addCardRow != null ? _addCardRow.GetComponent<Button>() : null;
        _selectedCardRowLabel = _selectedCardRow != null ? _selectedCardRow.GetComponentInChildren<TMP_Text>(true) : null;
        _savedCardRowTemplateLabel = _savedCardRowTemplate != null ? _savedCardRowTemplate.GetComponentInChildren<TMP_Text>(true) : null;
        _addCardRowLabel = _addCardRow != null ? _addCardRow.GetComponentInChildren<TMP_Text>(true) : null;
        PrepareSavedCardListRoot();
        _nameInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "NameInputField");
        _cardInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "CardInputField");
        _dateInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "DateInputField");
        _addCardCvvInputField = FindChildComponentIn<TMP_InputField>(_addCardWindow, "CVVInputField");
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

        if (_rememberCardButton != null)
        {
            _rememberCardButton.onClick.RemoveListener(ToggleRememberCard);
            _rememberCardButton.onClick.AddListener(ToggleRememberCard);
        }

        RefreshRememberCardStyle();
    }

    private void BindInputFields()
    {
        BindInputField(_nameInputField);
        BindNumericInputField(_cardInputField);
        BindDateInputField(_dateInputField);
        BindNumericInputField(_addCardCvvInputField, CvvDigitCount);
        ClearInputFieldPlaceholder(_addCardCvvInputField);
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
        RefreshRequiredFieldLabels();
    }

    public void SelectMobilePayment()
    {
        _usingCardPayment = false;
        RefreshRequiredFieldLabels();
    }

    public void ShowAddCardForm()
    {
        _savedCardListExpanded = false;
        ShowState(TransactionState.AddCard);
    }

    private void ToggleRememberCard()
    {
        _rememberCard = !_rememberCard;
        RefreshRememberCardStyle();
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
            ExpiryDate = GetInputText(_dateInputField),
            Remembered = _rememberCard
        };

        _savedCards.Add(card);
        _selectedCardIndex = _savedCards.Count - 1;
        _usingCardPayment = true;
        _savedCardListExpanded = false;

        if (_nameInputField != null) _nameInputField.text = string.Empty;
        if (_cardInputField != null) _cardInputField.text = string.Empty;
        if (_dateInputField != null) _dateInputField.text = string.Empty;
        if (_addCardCvvInputField != null) _addCardCvvInputField.text = string.Empty;
        RefreshPaymentButtons();
    }

    private void CacheRequiredFieldLabels()
    {
        _nameInputFieldLabel = FindTitleText(_nameInputField);
        _cardInputFieldLabel = FindTitleText(_cardInputField);
        _dateInputFieldLabel = FindTitleText(_dateInputField);
        _addCardCvvInputFieldLabel = FindTitleText(_addCardCvvInputField);

        _nameInputFieldLabelText = GetRequiredFieldLabelText(_nameInputFieldLabel, _nameInputFieldLabelText);
        _cardInputFieldLabelText = GetRequiredFieldLabelText(_cardInputFieldLabel, _cardInputFieldLabelText);
        _dateInputFieldLabelText = GetRequiredFieldLabelText(_dateInputFieldLabel, _dateInputFieldLabelText);
        _addCardCvvInputFieldLabelText = GetRequiredFieldLabelText(_addCardCvvInputFieldLabel, _addCardCvvInputFieldLabelText);
    }

    private void RefreshRequiredFieldLabels()
    {
        SetRequiredFieldLabel(_nameInputFieldLabel, _nameInputFieldLabelText, !HasText(_nameInputField));
        SetRequiredFieldLabel(_cardInputFieldLabel, _cardInputFieldLabelText, GetDigits(GetInputText(_cardInputField)).Length < 4);
        SetRequiredFieldLabel(_dateInputFieldLabel, _dateInputFieldLabelText, GetDigits(GetInputText(_dateInputField)).Length < ExpiryDateDigitCount);
        SetRequiredFieldLabel(_addCardCvvInputFieldLabel, _addCardCvvInputFieldLabelText, GetDigits(GetInputText(_addCardCvvInputField)).Length < CvvDigitCount);
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

        return true;
    }

    private void RefreshPaymentButtons()
    {
        SetText(_cardButtonLabel, EmptyCardButtonText);
        SetText(_mobileButtonLabel, MobileButtonText);
        RefreshSavedCardList();

        if (_usingCardPayment && _savedCards.Count > 0)
            SelectCardPayment();
        else
            SelectMobilePayment();
    }

    private void PrepareSavedCardListRoot()
    {
        if (_savedCardListSource == null)
            return;

        _savedCardListSource.enabled = false;
        foreach (Transform child in _savedCardListSource.transform)
        {
            bool isPrefabCardRow =
                child.gameObject == _selectedCardRow ||
                child.gameObject == _savedCardRowTemplate ||
                child.gameObject == _addCardRow;
            child.gameObject.SetActive(child == (_savedCardListTitle != null ? _savedCardListTitle.transform : null) || isPrefabCardRow);
        }

        if (_savedCardListTitle != null)
        {
            _savedCardListTitle.text = "Kortit";
            _savedCardListTitle.color = CardRowTextColor;
        }

        SetText(_addCardRowLabel, AddAnotherCardText);
        SetCardRowActive(_selectedCardRow, false);
        SetCardRowActive(_savedCardRowTemplate, false);
        SetCardRowActive(_addCardRow, false);

        Image background = _savedCardListSource.GetComponent<Image>();
        if (background != null)
        {
            background.color = Color.clear;
            background.raycastTarget = false;
        }
    }

    private void RefreshSavedCardList()
    {
        if (_savedCardListRoot == null || _selectedCardRow == null || _savedCardRowTemplate == null || _addCardRow == null)
            return;

        for (int i = 0; i < _savedCardRows.Count; i++)
        {
            if (_savedCardRows[i] != null)
                Destroy(_savedCardRows[i]);
        }
        _savedCardRows.Clear();

        if (_savedCards.Count > 0)
            _selectedCardIndex = Mathf.Clamp(_selectedCardIndex, 0, _savedCards.Count - 1);
        else
            _selectedCardIndex = -1;

        if (_selectedCardIndex < 0)
            return;

        if (!_savedCardListExpanded)
        {
            int selectedIndex = _selectedCardIndex;
            ConfigurePrefabCardRow(_selectedCardRow, _selectedCardRowButton, _selectedCardRowLabel, GetSavedCardText(_savedCards[selectedIndex]), ToggleSavedCardList);
            SetCardRowActive(_selectedCardRow, true);
            SetCardRowActive(_addCardRow, false);
            return;
        }

        SetCardRowActive(_selectedCardRow, false);
        for (int i = 0; i < _savedCards.Count; i++)
        {
            int cardIndex = i;
            GameObject cardRow = Instantiate(_selectedCardRow, _savedCardListRoot);
            cardRow.name = "SavedCardRow" + cardIndex;
            cardRow.transform.SetSiblingIndex(_savedCardRowTemplate.transform.GetSiblingIndex() + cardIndex + 1);
            AlignClonedCardRow(cardRow.GetComponent<RectTransform>(), _selectedCardRow.GetComponent<RectTransform>(), cardIndex);
            ConfigurePrefabCardRow(cardRow, cardRow.GetComponent<Button>(), cardRow.GetComponentInChildren<TMP_Text>(true), GetSavedCardText(_savedCards[cardIndex]), () => SelectSavedCard(cardIndex));
            SetCardRowActive(cardRow, true);
            _savedCardRows.Add(cardRow);
        }

        _addCardRow.transform.SetSiblingIndex(_savedCardRowTemplate.transform.GetSiblingIndex() + _savedCards.Count + 1);
        AlignClonedCardRow(_addCardRow.GetComponent<RectTransform>(), _selectedCardRow.GetComponent<RectTransform>(), _savedCards.Count);
        ConfigurePrefabCardRow(_addCardRow, _addCardRowButton, _addCardRowLabel, AddAnotherCardText, ShowAddCardForm);
        SetCardRowActive(_addCardRow, true);
    }

    private void ConfigurePrefabCardRow(GameObject row, Button button, TMP_Text label, string labelText, UnityEngine.Events.UnityAction onClick)
    {
        if (row == null)
            return;

        SetText(label, labelText);
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    private void AlignClonedCardRow(RectTransform row, RectTransform template, int rowIndex)
    {
        if (row == null || template == null)
            return;

        float rowHeight = template.anchorMax.y - template.anchorMin.y;
        float rowGap = rowHeight * 0.28f;
        float top = 1f - rowIndex * (rowHeight + rowGap);
        row.anchorMin = new Vector2(template.anchorMin.x, Mathf.Max(0f, top - rowHeight));
        row.anchorMax = new Vector2(template.anchorMax.x, top);
        row.anchoredPosition = Vector2.zero;
        row.sizeDelta = template.sizeDelta;
        row.pivot = template.pivot;
    }

    private void SetCardRowActive(GameObject row, bool isActive)
    {
        if (row != null)
            row.SetActive(isActive);
    }

    private void ToggleSavedCardList()
    {
        if (_savedCards.Count == 0)
            return;

        _savedCardListExpanded = !_savedCardListExpanded;
        RefreshSavedCardList();
    }

    private void SelectSavedCard(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= _savedCards.Count)
            return;

        _selectedCardIndex = selectedIndex;
        _usingCardPayment = true;
        _savedCardListExpanded = false;
        SelectCardPayment();
        RefreshSavedCardList();
    }

    private void RefreshRememberCardStyle()
    {
        if (_rememberCardBackground != null)
            _rememberCardBackground.color = _rememberCard ? RememberCardEnabledColor : RememberCardDisabledColor;
    }

    private void RemoveTemporaryCards()
    {
        _savedCards.RemoveAll(card => !card.Remembered);
        _selectedCardIndex = _savedCards.Count > 0 ? Mathf.Clamp(_selectedCardIndex, 0, _savedCards.Count - 1) : -1;
        _usingCardPayment = _savedCards.Count > 0;
        _savedCardListExpanded = false;
    }

    private string GetSavedCardText(SavedCard card)
    {
        return "VISA  " + card.DisplayNumber + "    " + card.ExpiryDate;
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

    private GameObject FindChildGameObjectIn(GameObject parent, string childName)
    {
        if (parent == null)
            return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child.gameObject;
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
