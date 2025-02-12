using System.Collections;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to display current daily tasks progress as a small popup window.
/// </summary>
public class DailyTaskProgressPopup : MonoBehaviour
{
    [Header("Player Daily Task Progress Popup")]
    [SerializeField] private RectTransform _progressPopupDailyTaskContainer;
    [SerializeField] private RectTransform _progressPopupDailyTaskVisibleLocation;
    [SerializeField] private RectTransform _progressPopupDailyTaskHiddenLocation;
    [Space]
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
    [Space]
    [SerializeField] private float _progressPopupContainerShowCooldown = 5f;
    [Tooltip("The time it takes for the popup window to move between positions.")]
    [SerializeField] private float _progressPopupContainerMoveTime = 0.75f;
    [SerializeField] private float _progressPopupContainerStopTime = 2.5f;
    [Tooltip("Used to animate how the popup window moves between positions.")]
    [SerializeField] private AnimationCurve _progressPopupContainerAnimationCurve;

    private bool _progressPopupCooldown = false;

    private void Start()
    {
        _progressPopupDailyTaskContainer.position = _progressPopupDailyTaskHiddenLocation.position;
        _progressPopupDailyTaskContainer.gameObject.SetActive(false);
        _progressPopupProgressContainer.SetActive(true);
        _progressPopupRewardContainer.SetActive(false);
    }

    private void OnEnable()
    {
        DailyTaskProgressManager.OnTaskProgressed += ShowProgressPopup;
    }

    private void OnDisable()
    {
        DailyTaskProgressManager.OnTaskProgressed -= ShowProgressPopup;
    }

    private void OnDestroy()
    {
        DailyTaskProgressManager.OnTaskProgressed -= ShowProgressPopup;
    }

    private void ShowProgressPopup()
    {
        var task = DailyTaskProgressManager.Instance.CurrentPlayerTask;

        if (!_progressPopupCooldown)
        {
            _progressPopupCooldown = true;
            StartCoroutine(MoveProgressPopupContainer());
        }

        if (task.TaskProgress >= task.Amount)
            SetProgressPopupDone(task);
        else
            SetProgressPopup(task);
    }

    /// <summary>
    /// Moves the ProgressPopup window to screen from hidden position <br></br>
    /// to visible, waits for a specifide time and then moves it self back <br></br>
    /// to hidden position.
    /// </summary>
    private IEnumerator MoveProgressPopupContainer()
    {
        int phase = 1;

        _progressPopupDailyTaskContainer.gameObject.SetActive(true);

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

                    _progressPopupDailyTaskContainer.position = Vector3.Lerp(_progressPopupDailyTaskHiddenLocation.position, _progressPopupDailyTaskVisibleLocation.position, curve);

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

        _progressPopupDailyTaskContainer.gameObject.SetActive(false);
        _progressPopupRewardContainer.SetActive(false);
        StartCoroutine(ProgressPopupCooldownTimer());
    }

    private void SetProgressPopup(PlayerTask task)
    {
        _progressPopupProgressContainer.SetActive(true);
        _progressPopupDailyTaskShortDescription.text = task.Title;
        _progressPopupDailyTaskValue.text = $"{task.TaskProgress}/{task.Amount}";
        _progressPopupDailyTaskFillImage.fillAmount = ((float)task.TaskProgress / (float)task.Amount);
    }

    private void SetProgressPopupDone(PlayerTask task)
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
