using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Popup : MonoBehaviour
{
    public static Popup Instance;

    public enum PopupWindowType
    {
        Accept,         //Accept task window
        Cancel,         //Cancel task window
        ClanMilestone,  //Clan milestone reward info window
    }

    [Header("Popup Settings")]
    [Tooltip("Assign the existing popup GameObject in the scene here.")]
    [SerializeField] private GameObject popupGameObject;
    [Space]
    [SerializeField] private GameObject _taskAcceptPopup;
    [SerializeField] private RectTransform _taskAcceptMovable;
    [Space]
    [SerializeField] private GameObject _taskCancelPopup;
    [Space]
    [Tooltip("Set every TMP text element here that is supposed to show a message from code.")]
    [SerializeField] private List<TextMeshProUGUI> _messageTexts;
    [Space]
    [SerializeField] private List<Button> _cancelButtons;
    [SerializeField] private List<Button> _acceptButtons;

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

    public static IEnumerator RequestPopup(string message, DailyTaskClanReward.ClanRewardData? clanRewardData, PopupWindowType type, Vector2? anchorLocation, System.Action<bool> callback)
    {
        if (Instance == null)
        {
            Debug.LogError("Popup instance is not set.");
            yield break;
        }

        Instance._result = null;
        Instance.SwitchWindow(type);
        if (anchorLocation != null)
            Instance.MoveMovableWindow(anchorLocation.Value, type);

        if (clanRewardData != null)
            Instance.SetClanMilestone(clanRewardData.Value.RewardImage, clanRewardData.Value.RewardAmount);

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
