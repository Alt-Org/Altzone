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
    [SerializeField] private GameObject _taskCancelPopup;
    [Space]
    [Tooltip("Set every TMP text element here that is supposed to show a message from code.")]
    [SerializeField] private List<TextMeshProUGUI> _messageTexts;
    [Space]
    [SerializeField] private List<Button> _cancelButtons;
    [SerializeField] private List<Button> _acceptButtons;
    [Space]
    [SerializeField] private TMP_Text _acceptConfirmButtonText;

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
                Instance.SetTaskAcceptImage(data.Value.OwnPage);
        }

        // Show the popup and get the result
        yield return Instance.StartCoroutine(Instance.ShowPopup(message));
        callback(Instance._result.Value); // Use the updated _result
    }

    private IEnumerator ShowPopup(string message)
    {
        // Start fade in
        if (_fadeOutCoroutine != null)
            StopCoroutine(_fadeOutCoroutine);

        _fadeInCoroutine = StartCoroutine(FadeIn());

        // Set the message text
        SetMessage(message);

        // Wait until one of the buttons is pressed
        yield return new WaitUntil(() => _result.HasValue);

        // Start fade out
        if (_fadeInCoroutine != null)
            StopCoroutine(_fadeInCoroutine);

        _fadeOutCoroutine = StartCoroutine(FadeOut());

        Debug.Log($"Popup result: {_result}"); // Log the result for debugging
    }

    private void SetTaskAcceptImage(PlayerTask data)
    {
        _taskAcceptImage.sprite = _cardImageReference.GetTaskImage(data);
    }

    private void SwitchWindow(PopupWindowType type)
    {
        _taskAcceptPopup.SetActive(type == PopupWindowType.Accept);
        _taskCancelPopup.SetActive(type == PopupWindowType.Cancel);
        _clanMilestonePopup.SetActive(type == PopupWindowType.ClanMilestone);
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

    private void SetMessage(string message)
    {
        foreach (var textItem in _messageTexts)
        {
            if(textItem.IsActive())
                textItem.text = message;
        }
    }

    private void SetClanMilestone(Sprite sprite, int rewardAmount) //TODO: Uncomment code when clan milestone images are available.
    {
        //_clanMilestoneRewardImage.sprite = sprite;
        _clanMilestoneRewardAmountText.text = $"{rewardAmount}x";
    }
}
