using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class WeekMood : AltMonoBehaviour
{
    [SerializeField] private Sprite[] _moodImages;
    [SerializeField] private GameObject[] _weekMoods;

    List<string> moodList = new List<string> { "", "", "", "", "", "", "" };
    List<string> moods = new List<string> { "Blank", "Love", "Playful", "Joy", "Sad", "Angry" };

    public delegate void PlayerDataCallback(PlayerData playerData);
    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        //LoadMoods();
        Test();
    }

    private void Test()
    {
        /*_weekMoods[0].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[1].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[2].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[3].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[4].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[5].GetComponent<Image>().sprite = _moodImages[0];
        _weekMoods[6].GetComponent<Image>().sprite = _moodImages[0];*/

        for (int i = 0; i < _weekMoods.Length; i++)
        {
            switch (moodList[i])
            {
                case "Blank":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[0];
                    break;
                case "Love":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[1];
                    break;
                case "Playful":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[2];
                    break;
                case "Joy":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[3];
                    break;
                case "Sad":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[4];
                    break;
                case "Angry":
                    _weekMoods[i].GetComponent<Image>().sprite = _moodImages[5];
                    break;
            }
        }
    }
}
