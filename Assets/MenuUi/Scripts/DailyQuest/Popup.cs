using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;
using MenuUI.Scripts;
using System.ComponentModel;

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

    /// <summary>
    /// Enum used for popup results
    /// </summary>
    public enum ResultType
    {
        Null = 0,
        /// <summary>
        /// (e.g. confirm or a correct answer)
        /// </summary>
        Accept = 1,
        /// <summary>
        /// (e.g. close)
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// If the popup got a result, but shouldn't close the window
        /// </summary>
        Normal,
        
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

    private ResultType _result;

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
            acceptButton.onClick.AddListener(() => _result = ResultType.Accept);

        foreach (var cancelButton in _cancelButtons)
            cancelButton.onClick.AddListener(() => _result = ResultType.Cancel);
    }

    public static IEnumerator RequestPopup(string message, PopupData? data, string currentTaskId, PopupWindowType type, System.Action<ResultType> callback)
    {
        Debug.LogWarning("REQUEST POPUP");
        if (Instance == null)
        {
            Debug.LogError("Popup instance is not set.");
            yield break;
        }

        Instance._result = ResultType.Null;
        Instance.SwitchWindow(type);

        if (data != null)
        {

            Debug.LogWarning("DATA NOT NULL");
            if (data.Value.Type == PopupData.PopupDataType.OwnTask)
            {
                Debug.LogWarning("OWN TASK");
                // If this is a new task
                if (currentTaskId == null)
                {
                    Instance._acceptConfirmButtonText.text = "Valitse";
                    Instance.ResetOptionButtons();
                }
                // If there is already a task running
                else
                {
                    Instance._acceptConfirmButtonText.text = "Vaihda Tehtävä";
                }
                    
            }
            Debug.LogWarning("PopupREQUIEST");
            if (data.Value.Location != null)
                Instance.MoveMovableWindow(data.Value.Location.Value, type);

            if (data.Value.ClanRewardData != null)
                Instance.SetClanMilestone(data.Value.ClanRewardData.Value.RewardImage, data.Value.ClanRewardData.Value.RewardAmount);

            if (data.Value.OwnPage != null)
            {
                Debug.LogWarning("OWNPAGEAS");
                Instance.SetTaskImage(data.Value.OwnPage, type);
                Instance.SetTaskDescription(data.Value.OwnPage);
                Instance.SetTaskRewardTexts(data.Value.OwnPage);
                Instance.SetPopupTaskColor(data.Value.OwnPage, data.Value.Type);
            }

            if (data.Value.Type == PopupData.PopupDataType.MultipleChoice)
            {
                // SetOptionButtons no matter if the task is a  new one or already on
                Instance.SetOptionButtons(data.Value.OwnPage);
            }

            if (data.Value.Type == PopupData.PopupDataType.CancelTask)
            {
                Instance._messageTexts[1].text = message; // Set cancel task text
            }
        }

        // Show the popup and get the result
        yield return Instance.StartCoroutine(Instance.ShowPopup(data.Value.OwnPage));
        callback(Instance._result);

    }

    /// <summary>
    /// Show the popup and close it after getting ResultType.Cancel or ResultType.Accept
    /// </summary>
    /// <param name="task">The player task</param>
    /// <returns></returns>
    private IEnumerator ShowPopup(PlayerTask task)
    {
        // Start fade in
        if (_fadeOutCoroutine != null)
            StopCoroutine(_fadeOutCoroutine);

        _fadeInCoroutine = StartCoroutine(FadeIn());

        // Set the message text
        SetMessage(task);

        // Wait until a the _result is ResultType.Cancel or ResultType.Accept
        // (so wait until the player has either selected the correct answer or want's to close the window)
        yield return new WaitUntil(() => _result == ResultType.Cancel || _result == ResultType.Accept);

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
        if (task == null) return;

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


    /// <summary>
    /// Set's the option buttons back to interactable and disables the gameobject
    /// </summary>
    private void ResetOptionButtons()
    {
        foreach (var button in _optionButtons)
        {
            button.interactable = true;
            button.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Initializes the multiple choice option buttons
    /// </summary>
    /// <param name="data">The player task</param>
    private void SetOptionButtons(PlayerTask data)
    {

        // Remove the listeners from the buttons, to avoid bugs
        foreach (var button in _optionButtons)
        {
            button.onClick.RemoveAllListeners();
        }
            

        // Get the multiple choice options
        List<string> options = MultipleChoiceOptions.Instance.GetTaskOptions(data);

        // Commented this out, because it seems unnecessary and was causing bugs
        // (e.g. when the window was reopened after player selected a wrong answer,
        // the order of the buttons was shuffled and the wrong button was grayed out)
        //Shuffle the list
        /*int n = options.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            string option = options[k];
            options[k] = options[n];
            options[n] = option;
        }*/

        // Loop through the option buttons
        for (int i = 0; i < _optionButtons.Count; i++)
        {
            // Make sure the loop does not go over the amount of options (if there are more buttons than options)
            if (i < options.Count)
            {
                string option = options[i];
                Button button = _optionButtons[i];
                button.GetComponentInChildren<TextMeshProUGUI>().text = option;

                // Add a listener for the OnClick event on the button
                button.onClick.AddListener(() =>
                {
                    // If on cooldown, don't allow the use
                    if (_isOnCooldown) return;


                    // Get the result for the option specified in the MultipleChoiceOptions asset
                    _result = MultipleChoiceOptions.Instance.GetResult(data, option);

                    // If the option was wrong 
                    if (_result == ResultType.Normal)
                    {
                        // Disable the button and inform player that it was the wrong answer
                        button.interactable = false;
                        StartCoroutine(Cooldown(5f));
                        //SignalBus.OnChangePopupInfoSignal("Väärä vastaus, yritä uudestaan.");
                    }
                    // If the option was correct
                    else if(_result == ResultType.Accept)
                    {
                        if (TaskEducationStoryType.WhereGameHappens == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationStoryType
                        && data.EducationStoryType == TaskEducationStoryType.WhereGameHappens)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationStoryType.WhereGameHappens, "1");
                        else if(TaskEducationCultureType.GamesGenreTypes == DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationCultureType
                        && data.EducationCultureType == TaskEducationCultureType.GamesGenreTypes)
                            DailyTaskProgressManager.Instance.UpdateTaskProgress(TaskEducationCultureType.GamesGenreTypes, "1");
                    }
                });
                _optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // Disable unnecessary buttons if there are any (buttons with no option)
                _optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator Cooldown(float seconds)
    {
        _isOnCooldown = true;
        _cooldownText.gameObject.SetActive(true);

        List<Button> interactableButtons = new List<Button>();

        // Loop through every button
        foreach (var button in _optionButtons)
        {
            // If an option button is interactable, add it to the list
            if (button.IsInteractable())
            {
                interactableButtons.Add(button);
                // Disable option selection for every button
                button.interactable = false;
            }
        }
            
        // Cooldown
        float timeLeft = seconds;
        while (timeLeft > 0)
        {
            //_cooldownText.text = $"{Mathf.CeilToInt(timeLeft)}s";
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
            {
                _cooldownText.text = "Wrong answer, try again.";
            }
            else
            {
                _cooldownText.text = "Väärä vastaus, yritä uudestaan.";
            }
                
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        // Enable option selection back for the interactaple option buttons
        foreach (var button in interactableButtons)
            button.interactable = true;

        _cooldownText.gameObject.SetActive(false);
        _isOnCooldown = false;
    }
}
