using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskRecogniseSymbolic : DailyTaskLongPress
{
    [SerializeField] private string _questionOption1;
    [SerializeField] private string _questionOption2;
    [SerializeField] private string _questionOption3;

    private Sprite _questionImage = null;

    public delegate void ActivateSymbolicQuestion(Sprite symbolicImage, string option1, string option2, string option3);
    public static event ActivateSymbolicQuestion OnActivateSymbolicQuestion;

    protected override void Start()
    {
        base.Start();
        _questionImage = GetComponent<Image>().sprite;
    }

    protected override void HandleLongPress()
    {
        OnActivateSymbolicQuestion?.Invoke(_questionImage, _questionOption1, _questionOption2, _questionOption3);
    }

}
