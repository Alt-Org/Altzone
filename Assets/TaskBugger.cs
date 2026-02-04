using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskBugger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _longClickStartThresholdTime = 0.2f;
    [SerializeField] private float _longClickThresholdTime = 3f;
    [SerializeField] private TextMeshProUGUI _buggedText;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Button _button;
    private bool _isHeldDown = false;

    [SerializeField] private GameObject _wheelPrefab;

    private GameObject _wheel;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (ProgressWheelHandler.Instance == null) InstantiateProgressWheel();
        else _wheel = ProgressWheelHandler.Instance.gameObject;
    }

    private void OnEnable()
    {
        StartCoroutine(ToggleBug());
    }

    //Instantiate wheel
    private void InstantiateProgressWheel()
    {
        _wheel = Instantiate(_wheelPrefab, _canvas.transform);
        _wheel.SetActive(false);
    }

    private bool CurrentTaskIsFindBug() {
        return (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug);
    }

    //Process of pressing bug in task
    private void ClickBuggedText(Vector3 clickPosition) => StartCoroutine(ClickBuggedTextCoroutine(clickPosition));

    private IEnumerator ClickBuggedTextCoroutine(Vector3 clickPosition)
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

    //Converts screenpoint(click) to worldpoint
    private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector3 worldPoint
        );
        return worldPoint;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHeldDown = true;
        ClickBuggedText(ScreenToWorldPoint(eventData.position));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHeldDown = false;
    }
}
