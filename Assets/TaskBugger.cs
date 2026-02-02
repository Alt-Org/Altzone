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
    private float progress;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        InstantiateProgressWheel();
    }

    private void OnEnable()
    {
        StartCoroutine(ToggleBug());
    }

    private void InstantiateProgressWheel()
    {
        _wheel = Instantiate(_wheelPrefab, _canvas.transform);
        _wheel.SetActive(false);
    }

    private void StartProgressWheelAtPosition(Vector3 position)
    {
        if (_wheel.activeSelf)
            return;
        _wheel.SetActive(true);
        _wheel.transform.position = position;
        _wheel.GetComponent<Image>().fillAmount = 0f;
        Debug.Log("ProgressWheel started at position: " + position);
    }

    private void DeactivateProgressWheel()
    {
        if (_wheel.activeSelf)
            _wheel.SetActive(false);
    }

    private bool currentTaskIsFindBug() {
        return (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug);
    }

    private void ClickBuggedText(Vector3 clickPosition) => StartCoroutine(ClickBuggedTextCoroutine(clickPosition));

    private IEnumerator ClickBuggedTextCoroutine(Vector3 clickPosition)
    {
        if (currentTaskIsFindBug())
        {
            float timer = 0f;
            while (_isHeldDown)
            {
                timer += Time.deltaTime;
                bool findBugTaskStarted = (timer > _longClickStartThresholdTime);
                bool findBugTaskCompleted = (timer >= _longClickThresholdTime);

                if(findBugTaskStarted)
                {
                    StartProgressWheelAtPosition(clickPosition);
                    progress = Mathf.Lerp(0, 1, timer/_longClickThresholdTime);
                    _wheel.GetComponent<Image>().fillAmount = progress;
                }

                if (findBugTaskCompleted)
                {
                    DeactivateProgressWheel();
                    DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug, "1");
                    yield return new WaitUntil(() => DailyTaskProgressManager.Instance.CurrentPlayerTask == null);
                    StartCoroutine(ToggleBug());
                    yield break;
                }

                yield return null;
            }
            DeactivateProgressWheel();
        }
        else
        StartCoroutine(ToggleBug());
    }

    private IEnumerator ToggleBug()
    {
        yield return null;
        if (currentTaskIsFindBug())
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
