using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomQuestionData
{

    [HideInInspector]
    public string Question
    {
        get
        {
            return question;
        }
    }

    [SerializeField] private string question;
    [SerializeField] private string englishQuestion;
    public List<RandomQuestionAnswer> answers;

}

[System.Serializable]
public class RandomQuestionAnswer
{
    [HideInInspector]
    public string Answer
    {
        get
        {
            return answer;
        }
    }

    [SerializeField]private string answer;
    [SerializeField]private string englishAnswer;
    public Color color;
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
