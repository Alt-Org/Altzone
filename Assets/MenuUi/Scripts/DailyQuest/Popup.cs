using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;

public class Popup : MonoBehaviour
{
    public static Popup Instance;

    public enum PopupWindowType
    {
        Accept,         //Accept task window
        Cancel,         //Cancel task window
        ClanMilestone,  //Clan milestone reward info window
        MultipleChoice, //Multiple choice task window
    }

    [SerializeField] private DailyTaskCardImageReference _cardImageReference;

    [Header("Popup Settings")]
    [Tooltip("Assign the existing popup GameObject in the scene here.")]
    [SerializeField] private GameObject popupGameObject;
    [Space]
    [SerializeField] private GameObject _taskAcceptPopup;
    [SerializeField] private RectTransform _taskAcceptMovable;
    [SerializeField] private Image _taskAcceptImage;
    [Space]
    [SerializeField] private Image _taskAcceptColorImage;
    [SerializeField] private Image _taskCancelColorImage;
    [Space]
    [SerializeField] private GameObject _taskCancelPopup;
    [Space]
    [Tooltip("Set every TMP text element here that is supposed to show a message from code.")]
    [SerializeField] private List<TextMeshProUGUI> _messageTexts;
    [Space]
    [SerializeField] private List<Button> _cancelButtons;
    [SerializeField] private List<Button> _acceptButtons;
    [Space]
    [SerializeField] private TMP_Text _acceptConfirmButtonText;
    [Space]
    [SerializeField] private TextMeshProUGUI _taskDescription;
    [SerializeField] private TextMeshProUGUI _taskPointsText;
    [SerializeField] private TextMeshProUGUI _taskCoinsText;

    [Header("FadeIn/Out")]
    [SerializeField] private CanvasGroup _popupCanvasGroup;
    [SerializeField] private float _fadeTime = 0.3f;
    private float _fadeTimer = 0f;
    private Coroutine _fadeInCoroutine;
    private Coroutine _fadeOutCoroutine;

    [Header("Clan Milestone")]
    [SerializeField] private GameObject _clanMilestonePopup;
    [SerializeField] private RectTransform _clanMilestoneMovable;
    [Space]
    [SerializeField] private GameObject _clanMilestoneTopPosition;
    [SerializeField] private Image _clanMilestoneRewardImage;
    [SerializeField] private TMP_Text _clanMilestoneRewardAmountText;
    [SerializeField] private float _clanMilestoneRightDiff = 0.1f;

    [Header("Popup Colors")]
    [SerializeField] private Color _actionCategoryColor;
    [SerializeField] private Color _socialCategoryColor;
    [SerializeField] private Color _storyCategoryColor;
    [SerializeField] private Color _cultureCategoryColor;
    [SerializeField] private Color _ethicalCategoryColor;
    [SerializeField] private Color _defaultColor;

    [Header("Multiple choice")]
    [SerializeField] private GameObject _multipleChoicePopup;
    [SerializeField] private Image _taskMultipleChoiceColorImage;
    [SerializeField] private Image _multipleChoiceTaskImage;
    [SerializeField] private List<Button> _optionButtons;
    [SerializeField] private TextMeshProUGUI _cooldownText;
    private bool _isOnCooldown = false;

    private bool? _result;

    private void Awake()
    {
        // Ensure there's only one instance of Popup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure the popup starts disabled
        popupGameObject.SetActive(false);
    }

    private void Start()
    {
        //Set buttons
        foreach (var acceptButton in _acceptButtons)
            acceptButton.onClick.AddListener(() => _result = true);

        foreach (var cancelButton in _cancelButtons)
            cancelButton.onClick.AddListener(() => _result = false);
    }

    public static IEnumerator RequestPopup(string message, PopupData? data, string currentTaskId, PopupWindowType type, System.Action<bool> callback)
    {
        if (Instance == null)
        {
            Debug.LogError("Popup instance is not set.");
            yield break;
        }

        Instance._result = null;
        Instance.SwitchWindow(type);

        if (data != null)
        {
            if (data.Value.Type == PopupData.PopupDataType.OwnTask)
            {
                if (currentTaskId == null)
                    Instance._acceptConfirmButtonText.text = "Valitse";
                else
                    Instance._acceptConfirmButtonText.text = "Vaihda Tehtävä";
            }

            if (data.Value.Location != null)
                Instance.MoveMovableWindow(data.Value.Location.Value, type);

            if (data.Value.ClanRewardData != null)
                Instance.SetClanMilestone(data.Value.ClanRewardData.Value.RewardImage, data.Value.ClanRewardData.Value.RewardAmount);

            if (data.Value.OwnPage != null)
            {
                Instance.SetTaskImage(data.Value.OwnPage, type);
                Instance.SetTaskDescription(data.Value.OwnPage);
                Instance.SetTaskRewardTexts(data.Value.OwnPage);
                Instance.SetPopupTaskColor(data.Value.OwnPage, data.Value.Type);
            }

            if (data.Value.Type == PopupData.PopupDataType.MultipleChoice)
            {
                Instance.SetOptionButtons(data.Value.OwnPage);
            }
        }

        // Show the popup and get the result
        yield return Instance.StartCoroutine(Instance.ShowPopup(data.Value.OwnPage));
        callback(Instance._result.Value);

    }

    private IEnumerator ShowPopup(PlayerTask task)
    {
        // Start fade in
        if (_fadeOutCoroutine != null)
            StopCoroutine(_fadeOutCoroutine);

        _fadeInCoroutine = StartCoroutine(FadeIn());

        // Set the message text
        SetMessage(task);

        // Wait until one of the buttons is pressed
        yield return new WaitUntil(() => _result.HasValue);

        // Start fade out
        if (_fadeInCoroutine != null)
            StopCoroutine(_fadeInCoroutine);

        _fadeOutCoroutine = StartCoroutine(FadeOut());

        Debug.Log($"Popup result: {_result}"); // Log the result for debugging
    }


    private void SetTaskImage(PlayerTask data, PopupWindowType type)
    {
        switch (type)
        {
            case PopupWindowType.Accept:
                {
                    _taskAcceptImage.sprite = _cardImageReference.GetTaskImage(data);
                    return;
                }
            case PopupWindowType.MultipleChoice:
                {
                    _multipleChoiceTaskImage.sprite = _cardImageReference.GetTaskImage(data);
                    return;
                }
            default: return;
        }
    }

    private void SetTaskDescription(PlayerTask data)
    {
        _taskDescription.text = data.Content;
    }

    private void SetTaskRewardTexts(PlayerTask data)
    {
        _taskPointsText.text = data.Points.ToString();
        _taskCoinsText.text = data.Coins.ToString();
    }

    private void SetPopupTaskColor(PlayerTask data, PopupData.PopupDataType type)
    {
        Image targetImage = _taskAcceptColorImage;

        if (type == PopupData.PopupDataType.CancelTask) targetImage = _taskCancelColorImage;

        if (type == PopupData.PopupDataType.MultipleChoice) targetImage = _taskMultipleChoiceColorImage;

        Color taskColor = _defaultColor;

        switch (data.EducationCategory)
        {
            case EducationCategoryType.Action: taskColor = _actionCategoryColor; break;
            case EducationCategoryType.Social: taskColor = _socialCategoryColor; break;
            case EducationCategoryType.Story: taskColor = _storyCategoryColor; break;
            case EducationCategoryType.Culture: taskColor = _cultureCategoryColor; break;
            case EducationCategoryType.Ethical: taskColor = _ethicalCategoryColor; break;
            default: break;
        }

        targetImage.color = taskColor;
    }

    private void SwitchWindow(PopupWindowType type)
    {
        _taskAcceptPopup.SetActive(type == PopupWindowType.Accept);
        _taskCancelPopup.SetActive(type == PopupWindowType.Cancel);
        _clanMilestonePopup.SetActive(type == PopupWindowType.ClanMilestone);
        _multipleChoicePopup.SetActive(type == PopupWindowType.MultipleChoice);
    }

    private void MoveMovableWindow(Vector3 location, PopupWindowType type)
    {
        //Accept window.
        if (type == PopupWindowType.Accept)
            _taskAcceptMovable.position = location;

        //Clan milestone info window.
        else if (type == PopupWindowType.ClanMilestone)
        {
            float halfHeight = _clanMilestoneMovable.position.y - _clanMilestoneTopPosition.transform.position.y;
            _clanMilestoneMovable.position = location + new Vector3(-(Screen.width * _clanMilestoneRightDiff), halfHeight);
        }
    }

    private IEnumerator FadeIn()
    {
        popupGameObject.SetActive(true);

        while (_fadeTimer < _fadeTime)
        {
            _popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, (_fadeTimer / _fadeTime));
            _fadeTimer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while (_fadeTimer > 0f)
        {
            _popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, (_fadeTimer / _fadeTime));
            _fadeTimer -= Time.deltaTime;
            yield return null;
        }

        popupGameObject.SetActive(false);
    }

    private void SetMessage(PlayerTask task)
    {
        for (int i = 0; i < _messageTexts.Count; i++)
        {
            if (!_messageTexts[i].IsActive()) continue;

            if (i == 0) // First element should be the task title (for some reason)
            {
                _messageTexts[i].text = SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English // Simply, if english language is selected, show it in english
                    ? task.EnglishTitle
                    : task.Title;
            }
            else
            {
                _messageTexts[i].text = task.Content; // For others, just keep the original
            }
        }
    }

    private void SetClanMilestone(Sprite sprite, int rewardAmount) //TODO: Uncomment code when clan milestone images are available.
    {
        //_clanMilestoneRewardImage.sprite = sprite;
        _clanMilestoneRewardAmountText.text = $"{rewardAmount}x";
    }

    private void SetOptionButtons(PlayerTask data)
    {
        foreach (var button in _optionButtons)
            button.onClick.RemoveAllListeners();
        
        List<string> options = MultipleChoiceOptions.Instance.GetTaskOptions(data);

        //Shuffle the list
        int n = options.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            string option = options[k];
            options[k] = options[n];
            options[n] = option;
        }

        for (int i = 0; i < _optionButtons.Count; i++)
        {
            if (i < options.Count)
            {
                string option = options[i];
                _optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = option;
                _optionButtons[i].onClick.AddListener(() =>
                {
                    if (_isOnCooldown) return;

                    _result = MultipleChoiceOptions.Instance.GetResult(data, option);

                    if (_result.HasValue && _result.Value == false)
                        StartCoroutine(Cooldown(60f));
                });
                _optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                _optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator Cooldown(float seconds)
    {
        _isOnCooldown = true;
        _cooldownText.gameObject.SetActive(true);

        foreach (var button in _optionButtons)
            button.interactable = false;

        float timeLeft = seconds;
        while (timeLeft > 0)
        {
            _cooldownText.text = $"{Mathf.CeilToInt(timeLeft)}s";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        foreach (var button in _optionButtons)
            button.interactable = true;

        _cooldownText.gameObject.SetActive(false);
        _isOnCooldown = false;
    }
}
