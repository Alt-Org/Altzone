using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    private List<string> buttonPresses = new List<string>();
    private int score = 0;
    public TMP_Text scoreText; 

    void Start()
    {
        //ChatTask.OnButtonPressed += RecordButtonPress;
        UpdateScoreText();
    }

    //void RecordButtonPress(Button button)
    //{
    //    string buttonText = button.GetComponentInChildren<Text>().text;
    //    buttonPresses.Add(buttonText);

    //    if (buttonPresses.Count % 10 == 0)
    //    {
    //        AddScore(100); 
    //    }
    //}

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText(); 
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public List<string> GetButtonPresses()
    {
        return buttonPresses;
    }

    public int GetScore()
    {
        return score;
    }

    [ContextMenu("lisää pisteitä")]
    public void AddTestPoints()
    {
        AddScore(50); 
    }
}
