using System.Collections;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to display any daily task progress notification<br/>
/// as a small popup window where the UiOverlay is used.
/// </summary>
public class DailyTaskProgressPopup : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject _progressPopupDailyTaskContainer;
    [SerializeField] private RectTransform _progressPopupDailyTaskVisibleLocation;
    [SerializeField] private RectTransform _progressPopupDailyTaskHiddenLocation;
    [Space]
    [SerializeField] private GameObject _progressPopupTaskContainer;
    [SerializeField] private GameObject _progressPopupClanContainer;
    [Space]
    [Tooltip("Minimum time for the popup conatiner to stay in hidden position.")]
    [SerializeField] private float _progressPopupContainerShowCooldown = 5f;
    [Tooltip("The time it takes for the popup window to move between positions.")]
    [SerializeField] private float _progressPopupContainerMoveTime = 0.75f;
    [Tooltip("How long the popup conatiner stays in visible position.")]
    [SerializeField] private float _progressPopupContainerStopTime = 2.5f;
    [Tooltip("Used to animate how the popup window moves between positions.")]
    [SerializeField] private AnimationCurve _progressPopupContainerAnimationCurve;

    [Header("Task")]
    [SerializeField] private GameObject _progressPopupProgressContainer;
    [SerializeField] private GameObject _progressPopupRewardContainer;
    [Space]
    [SerializeField] private GameObject _progressPopupPointsRewardContainer;
    [SerializeField] private GameObject _progressPopupCoinsRewardContainer;
    [Space]
    [SerializeField] private TMP_Text _progressPopupDailyTaskShortDescription;
    [SerializeField] private TMP_Text _progressPopupDailyTaskValue;
    [SerializeField] private Image _progressPopupDailyTaskFillImage;
    [Space]
    [SerializeField] private TMP_Text _progressPopupPointsRewardValue;
    [SerializeField] private TMP_Text _progressPopupCoinsRewardValue;

    [Header("Clan")]
    [SerializeField] private Image _progressPopupClanMilestoneRewardImage;
    [SerializeField] private TMP_Text _progressPopupClanMilestoneRewardValue;

    private bool _progressPopupCooldown = false;
    private bool _taskPopupActive = false;
    private bool _clanPopupActive = false;
    private Coroutine _coroutineMovePopup = null;
    private Coroutine _coroutineCooldownPopup = null;
    private int _phase = 1;

    public enum ContainerType
    {
        Task,
        Clan,
    }

    private void Start()
    {
        Reset();
    }

    private void OnEnable()
    {
        DailyTaskProgressManager.OnTaskProgressed += ShowTaskProgressPopup;
        DailyTaskProgressManager.OnClanMilestoneProgressed += ShowClanMilestonePopup;
    }

    private void OnDisable()
    {
        DailyTaskProgressManager.OnTaskProgressed -= ShowTaskProgressPopup;
        DailyTaskProgressManager.OnClanMilestoneProgressed -= ShowClanMilestonePopup;
        Reset();
    }

    private void OnDestroy()
    {
        DailyTaskProgressManager.OnTaskProgressed -= ShowTaskProgressPopup;
        DailyTaskProgressManager.OnClanMilestoneProgressed -= ShowClanMilestonePopup;
    }

    private void Reset()
    {
        _progressPopupCooldown = false;
        _taskPopupActive = false;
        _clanPopupActive = false;
        _phase = 1;
        _progressPopupDailyTaskContainer.transform.position = _progressPopupDailyTaskHiddenLocation.position;
        _progressPopupDailyTaskContainer.SetActive(false);

        if (_coroutineMovePopup != null)
            StopCoroutine(_coroutineMovePopup);

        if (_coroutineCooldownPopup != null)
            StopCoroutine(_coroutineCooldownPopup);
    }

    private void ShowTaskProgressPopup()
    {
        if (_clanPopupActive)
            return;

        if (!_progressPopupCooldown)
        {
            _taskPopupActive = true;
            _progressPopupCooldown = true;
            _coroutineMovePopup = StartCoroutine(MoveProgressPopupContainer(ContainerType.Task));
        }

        var task = DailyTaskProgressManager.Instance.CurrentPlayerTask;

        if (task.TaskProgress >= task.Amount)
            SetTaskProgressPopupDone(task);
        else
            SetTaskProgressPopup(task);

        SwitchPopupContainer(ContainerType.Task);
    }

    private IEnumerator ShowClanMilestonePopup()
    {
        if (!_progressPopupCooldown || !_clanPopupActive)
        {
            _progressPopupCooldown = true;
            _clanPopupActive = true;

            while (_taskPopupActive)
                yield return null;

            _coroutineMovePopup = StartCoroutine(MoveProgressPopupContainer(ContainerType.Clan));
        }

        //_progressPopupClanMilestoneRewardImage.sprite = INSERT IMAGE HERE;
        _progressPopupClanMilestoneRewardValue.text = $"{999}x";

        SwitchPopupContainer(ContainerType.Clan);
    }

    /// <summary>
    /// Moves the ProgressPopup window to screen from hidden position <br/>
    /// to visible, waits for a specifide time and then moves it self back <br/>
    /// to hidden position.
    /// </summary>
    private IEnumerator MoveProgressPopupContainer(ContainerType type)
    {
        _phase = 1;

        _progressPopupDailyTaskContainer.SetActive(true);

        while (_phase < 4)
        {
            float timer = 0f;
            float curve = 0f;

            if (_phase == 1 || _phase == 3)
                while (timer < _progressPopupContainerMoveTime)
                {
                    if (_phase == 1)
                        curve = _progressPopupContainerAnimationCurve.Evaluate(timer / _progressPopupContainerMoveTime);
                    else //phase == 3
                        curve = _progressPopupContainerAnimationCurve.Evaluate(1f - (timer / _progressPopupContainerMoveTime));

                    _progressPopupDailyTaskContainer.transform.position = Vector3.Lerp(_progressPopupDailyTaskHiddenLocation.position, _progressPopupDailyTaskVisibleLocation.position, curve);

                    timer += Time.deltaTime;
                    yield return null;
                }
            else if (_phase == 2)
                while (timer < _progressPopupContainerStopTime)
                {
                    // If clan popup is waiting, proceed to pahse 3 to close the task popup.
                    if (_taskPopupActive && _clanPopupActive)
                        break;

                    timer += Time.deltaTime;
                    yield return null;
                }

            _phase++;
            yield return null;
        }

        _progressPopupDailyTaskContainer.SetActive(false);

        if (_taskPopupActive != _clanPopupActive)
            _coroutineCooldownPopup = StartCoroutine(ProgressPopupCooldownTimer(type));
        else if (_taskPopupActive)
            _taskPopupActive = false;
    }

    private void SwitchPopupContainer(ContainerType type)
    {
        _progressPopupTaskContainer.SetActive(type == ContainerType.Task);
        _progressPopupClanContainer.SetActive(type == ContainerType.Clan);
    }

    private void SetTaskProgressPopup(PlayerTask task)
    {
        _progressPopupProgressContainer.SetActive(true);
        _progressPopupRewardContainer.SetActive(false);

        _progressPopupDailyTaskShortDescription.text = task.Title;
        _progressPopupDailyTaskValue.text = $"{task.TaskProgress}/{task.Amount}";
        _progressPopupDailyTaskFillImage.fillAmount = ((float)task.TaskProgress / (float)task.Amount);
    }

    private void SetTaskProgressPopupDone(PlayerTask task)
    {
        _progressPopupProgressContainer.SetActive(false);
        _progressPopupRewardContainer.SetActive(true);

        _progressPopupPointsRewardContainer.SetActive(task.Points != 0);
        _progressPopupCoinsRewardContainer.SetActive(task.Coins != 0);

        _progressPopupDailyTaskShortDescription.text = task.Title;
        _progressPopupPointsRewardValue.text = "" + task.Points + " p";
        _progressPopupCoinsRewardValue.text = "" + task.Coins;
    }

    private IEnumerator ProgressPopupCooldownTimer(ContainerType type)
    {
        float time = 0;

        while (time < _progressPopupContainerShowCooldown)
        {
            if (type == ContainerType.Task && _clanPopupActive)
                break;

            time += Time.deltaTime;
            yield return null;
        }

        switch (type)
        {
            case ContainerType.Task: _taskPopupActive = false; break;
            case ContainerType.Clan: _clanPopupActive = false; break;
        }

        _progressPopupCooldown = false;
    }
}
