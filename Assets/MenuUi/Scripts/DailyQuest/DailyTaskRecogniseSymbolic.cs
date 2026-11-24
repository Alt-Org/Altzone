using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskRecogniseSymbolic : DailyTaskLongPress
{
    [SerializeField] private string _questionOption1Fi;
    [SerializeField] private string _questionOption1En;
    [SerializeField] private string _questionOption2Fi;
    [SerializeField] private string _questionOption2En;
    [SerializeField] private string _questionOption3Fi;
    [SerializeField] private string _questionOption3En;
    [SerializeField] private int _answer;

    private Sprite _questionImage = null;

    public delegate void ActivateSymbolicQuestion(Sprite symbolicImage, string option1, string option2, string option3, int answer);
    public static event ActivateSymbolicQuestion OnActivateSymbolicQuestion;

    private string QuestionOption1
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                return _questionOption1Fi;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                return _questionOption1En;
            else
                return _questionOption1Fi;
        }
    }

    private string QuestionOption2
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                return _questionOption2Fi;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                return _questionOption2En;
            else
                return _questionOption2Fi;
        }
    }

    private string QuestionOption3
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                return _questionOption3Fi;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                return _questionOption3En;
            else
                return _questionOption3Fi;
        }
    }

    protected override void Start()
    {
        base.Start();
        _questionImage = GetComponent<Image>().sprite;
    }

    protected override void HandleLongPress()
    {
        OnActivateSymbolicQuestion?.Invoke(_questionImage, QuestionOption1, QuestionOption2, QuestionOption3, _answer);
    }

}
