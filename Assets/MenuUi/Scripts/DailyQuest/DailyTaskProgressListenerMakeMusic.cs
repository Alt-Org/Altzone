using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System.Collections.Generic;

public class DailyTaskProgressListenerMakeMusic : DailyTaskProgressListener
{
    [SerializeField] private Button[] _buttons;

    private int[] _targetSequence = new int[] { 0, 0, 0, 2, 1, 1, 1, 3, 2, 2, 1, 1, 0 };
    private List<int> _currentSequence = new();

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Action;
        _educationCategoryActionType = TaskEducationActionType.MakeMusicWithButtons;

        for (int i = 0; i < _buttons.Length; i++)
        {
            int index = i;
            _buttons[index].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    private void OnButtonClick(int index)
    {
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask == null || DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType != _educationCategoryActionType) return;
        _currentSequence.Add(index);

        for (int i = 0; i < _currentSequence.Count; i++)
        {
            if (_currentSequence[i] != _targetSequence[i])
            {
                ResetTask();
                return;
            }
        }

        if (_currentSequence.Count == _targetSequence.Length)
        {
            CompleteTask();
        }
    }

    private void CompleteTask()
    {
        UpdateProgress("1");
        ResetTask();
    }

    private void ResetTask()
    {
        for (int i = 0; i < _currentSequence.Count; i++)
        {
            if (_currentSequence[i] != _targetSequence[i])
            {
                _currentSequence.RemoveAt(0);
                if(_currentSequence.Count > 0) i = -1;
                return;
            }
        }
    }
}
