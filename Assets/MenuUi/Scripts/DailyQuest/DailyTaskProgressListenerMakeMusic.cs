using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System.Collections;

public class DailyTaskProgressListenerMakeMusic : DailyTaskProgressListener
{
    [SerializeField] private Button[] _buttons;
    [SerializeField] private int _requiredClicks = 10;
    [SerializeField] private float _timeLimit = 6f;

    private int _currentClicks = 0;
    private float _elapsedTime = 0f;
    private bool _taskActive = false;
    private Coroutine _timer;

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Action;
        _educationCategoryActionType = TaskEducationActionType.MakeMusicWithButtons;

        foreach (Button button in _buttons)
        {
            button.onClick.AddListener(CountClicks);
        }
    }

    private void CountClicks()
    {
        if (!_taskActive)
        {
            _taskActive = true;
            _timer = StartCoroutine(Timer());
        }

        _currentClicks++;

        if (_currentClicks >= _requiredClicks)
        {
            CompleteTask();
        }
    }

    private IEnumerator Timer()
    {
        _elapsedTime = 0f;

        while (_elapsedTime < _timeLimit)
        {
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        ResetTask();
    }

    private void CompleteTask()
    {
        if (_timer != null)
            StopCoroutine(_timer);

        UpdateProgress("1");
        ResetTask();
    }

    private void ResetTask()
    {
        _taskActive = false;
        _elapsedTime = 0f;
        _currentClicks = 0;
    }
}
