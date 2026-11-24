using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomQuestionData
{
    public string Question
    {
        get
        {
            return SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _englishQuestion : _question;      
        }
    }

    [SerializeField] private string _question;
    [SerializeField] private string _englishQuestion;
    public List<RandomQuestionAnswer> answers;
}

[System.Serializable]
public class RandomQuestionAnswer
{
    public string Answer
    {
        get
        {
            return SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? _englishAnswer : _answer;
        }
    }

    [SerializeField]private string _answer;
    [SerializeField]private string _englishAnswer;
}

[CreateAssetMenu(fileName = "NewRandomQuestionConfig", menuName = "ALT-Zone/DailyTask/RandomQuestionConfig")]
public class RandomQuestionConfig : ScriptableObject
{
    private static RandomQuestionConfig _instance;

    public static RandomQuestionConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<RandomQuestionConfig>(nameof(RandomQuestionConfig));
            }

            return _instance;
        }
    }

    [SerializeField] private List<RandomQuestionData> _questions;

    public List<RandomQuestionData> GetRandomQuestions() => _questions;
}
