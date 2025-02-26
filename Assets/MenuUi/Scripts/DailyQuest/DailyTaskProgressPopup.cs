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
    private Coroutine _coroutineMovePopup = null;
    private Coroutine _coroutineCooldownPopup = null;

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
        _progressPopupDailyTaskContainer.transform.position = _progressPopupDailyTaskHiddenLocation.position;
        _progressPopupDailyTaskContainer.SetActive(false);

        if (_coroutineMovePopup != null)
            StopCoroutine(_coroutineMovePopup);

        if (_coroutineCooldownPopup != null)
            StopCoroutine(_coroutineCooldownPopup);
    }

    private void ShowTaskProgressPopup()
    {
        if (!_progressPopupCooldown)
        {
            _progressPopupCooldown = true;
            _coroutineMovePopup = StartCoroutine(MoveProgressPopupContainer());
        }

        var task = DailyTaskProgressManager.Instance.CurrentPlayerTask;

        if (task.TaskProgress >= task.Amount)
            SetTaskProgressPopupDone(task);
        else
            SetTaskProgressPopup(task);

        SwitchPopupContainer(ContainerType.Task);
    }

    private void ShowClanMilestonePopup()
    {
        if (!_progressPopupCooldown)
        {
            _progressPopupCooldown = true;
            _coroutineMovePopup = StartCoroutine(MoveProgressPopupContainer());
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
    private IEnumerator MoveProgressPopupContainer()
    {
        int phase = 1;

        _progressPopupDailyTaskContainer.SetActive(true);

        while (phase < 4)
        {
            float timer = 0, curve;

            if (phase == 1 || phase == 3)
                while (timer < _progressPopupContainerMoveTime)
                {
                    if (phase == 1)
                        curve = _progressPopupContainerAnimationCurve.Evaluate(timer / _progressPopupContainerMoveTime);
                    else //phase == 3
                        curve = _progressPopupContainerAnimationCurve.Evaluate(1f - (timer / _progressPopupContainerMoveTime));

                    _progressPopupDailyTaskContainer.transform.position = Vector3.Lerp(_progressPopupDailyTaskHiddenLocation.position, _progressPopupDailyTaskVisibleLocation.position, curve);

                    timer += Time.deltaTime;
                    yield return null;
                }
            else if (phase == 2)
                while (timer < _progressPopupContainerStopTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

            phase++;
            yield return null;
        }

        _progressPopupDailyTaskContainer.SetActive(false);
        _coroutineCooldownPopup = StartCoroutine(ProgressPopupCooldownTimer());
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

    private IEnumerator ProgressPopupCooldownTimer()
    {
        yield return new WaitForSeconds(_progressPopupContainerShowCooldown);

        _progressPopupCooldown = false;
    }
}
