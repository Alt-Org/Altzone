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
    private float progress;
    private Vector2 _clickPosition;

    private void OnEnable()
    {
        StartCoroutine(ToggleBug());
    }

    private void ClickBuggedText() => StartCoroutine(ClickBuggedTextCoroutine(_clickPosition));

    private IEnumerator ClickBuggedTextCoroutine(Vector2 clickPosition)
    {
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug)
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime;

                if(timer > _longClickStartThresholdTime){
                    if(_wheel == null)
                        if(ProgressWheelHandler.Instance != null && ProgressWheelHandler.Instance.Wheel)
                        _wheel = Instantiate(_wheelPrefab, transform);
                    if(!_wheel.activeSelf){
                        _wheel.SetActive(true);
                        _wheel.GetComponent<RectTransform>().position = _clickPosition;
                        progress = 0f;
                    }

                    progress = Mathf.Lerp(0, 1, timer/_longClickThresholdTime);
                    _wheel.GetComponent<Image>().fillAmount = progress;
                }

                if (_isHeldDown == false)
                    yield break;

                if (timer >= _longClickThresholdTime)
                {
                    DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug, "1");
                    yield return new WaitUntil(() => DailyTaskProgressManager.Instance.CurrentPlayerTask == null);
                    StartCoroutine(ToggleBug());
                    yield break;
                }

                yield return null;
            }
        }
        else
        StartCoroutine(ToggleBug());
    }

    private IEnumerator ToggleBug()
    {
        yield return null;
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
                    && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.FindBug)
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

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHeldDown = true;
        _clickPosition = eventData.pressPosition;
        ClickBuggedText();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHeldDown = false;
    }
}
