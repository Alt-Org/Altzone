using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System.Collections.Generic;
using MenuUi.Scripts.Window;

public class DailyTaskProgressListenerMakeMusic : DailyTaskProgressListener
{
    [SerializeField] private Button[] _buttonsToClick;
    [SerializeField] private NaviButton[] _naviButtonsToDisable;

    private int[] _targetSequence = new int[] { 0, 0, 0, 2, 1, 1, 1, 3, 2, 2, 1, 1, 0 };
    private List<int> _currentSequence = new();

    private List<NaviButton> _naviButtons = new();

    private void Awake()
    {
        _educationCategoryType = EducationCategoryType.Action;
        _educationCategoryActionType = TaskEducationActionType.MakeMusicWithButtons;

        for (int i = 0; i < _buttonsToClick.Length; i++)
        {
            // The index has to be a separate variable, because otherwise the added listener will always be the final "int i"
            int index = i;
            _buttonsToClick[index].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public override void SetState(PlayerTask task)
    {
        base.SetState(task);

        if (On)
        {
            StoreNaviButtons();
            UpdateNaviButtons(false);
            
        }
    }

    private void OnButtonClick(int index)
    {
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask == null || DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType != _educationCategoryActionType) return;
        if (_currentSequence.Count >= _targetSequence.Length) _currentSequence.RemoveAt(0);
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
        UpdateNaviButtons(true);
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

    /// <summary>
    /// Finds Interactable NaviButtons from the _naviButtonsToDisable list and stores them into their own _naviButtons list.
    /// </summary>
    private void StoreNaviButtons()
    {
        _naviButtons.Clear();

        foreach (NaviButton naviBtn in _naviButtonsToDisable)
        {
            // Make sure the naviBtn is Interactable before storing it into the list
            if (naviBtn.Interactable) _naviButtons.Add(naviBtn);
        }
    }

    /// <summary>
    /// Makes the NaviButtons interactable/not interactable
    /// </summary>
    /// <param name="interactable">If the NaviButton should be Interactable</param>
    private void UpdateNaviButtons(bool interactable)
    {
        if (_naviButtons.Count == 0) return;

        foreach (NaviButton naviBtn in _naviButtons)
        {
            naviBtn.Interactable = interactable;
        }
    }
}
