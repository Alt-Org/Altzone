using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskBugger : DailyTaskLongPress, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI _buggedText;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Button _button;
    private bool _isHeldDown = false;

    [SerializeField] private GameObject _wheelPrefab;

    private void OnEnable()
    {
        StartCoroutine(ToggleBug());
    }

    private bool CurrentTaskIsFindBug() {
        return (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug);
    }

    //Process of pressing bug in task
    private void ClickBuggedText(Vector3 clickPosition) => StartCoroutine(HoldDownTimer(clickPosition));

    protected override IEnumerator HoldDownTimer(Vector3 clickPosition)
    {
        if (CurrentTaskIsFindBug())
        {
            float timer = 0f;
            while (_isHeldDown)
            {
                timer += Time.deltaTime;
                bool findBugTaskStarted = (timer > _longClickStartThresholdTime);
                bool findBugTaskCompleted = (timer >= _longClickThresholdTime);

                if(findBugTaskStarted)
                {
                    ProgressWheelHandler.Instance.StartProgressWheelAtPosition(clickPosition, _longClickStartThresholdTime, _longClickThresholdTime);
                }

                if (findBugTaskCompleted)
                {
                    ProgressWheelHandler.Instance.DeactivateProgressWheel();
                    DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug, "1");
                    yield return new WaitUntil(() => DailyTaskProgressManager.Instance.CurrentPlayerTask == null);
                    StartCoroutine(ToggleBug());
                    yield break;
                }

                yield return null;
            }
            ProgressWheelHandler.Instance.DeactivateProgressWheel();
        }
        else
        StartCoroutine(ToggleBug());
    }

    private IEnumerator ToggleBug()
    {
        yield return null;
        if (CurrentTaskIsFindBug())
        {
            _buggedText.SetText("##Text_Parental_Internet_English");
            _toggle.enabled = false;
            //_button.enabled = true;
            //_button.onClick.AddListener(ClickBuggedText);
        }
        else
        {
            _buggedText.SetText("Salli InternetLinkit");
            _toggle.enabled = true;
            //_button.enabled = false;
            //_button.onClick.RemoveListener(ClickBuggedText);
        }
    }


    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!On)
            return;

        _oneShot = true;
        _isHeldDown = true;
        ClickBuggedText(ScreenToWorldPoint(eventData.position));
    }
}
