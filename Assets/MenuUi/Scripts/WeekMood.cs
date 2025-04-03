using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Altzone.Scripts.Model.Poco.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class WeekMood : AltMonoBehaviour
{
    // Sprites that are used to show the moods in profile.
    [SerializeField] private Sprite[] _moodImages;

    // Components that hold the sprite and shows the moods in profile.
    [SerializeField] private GameObject[] _weekMoods;

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the needed playerdata
        StartCoroutine(GetPlayerData(data => _playerData = data));

        ValuesToWeekMoods();
    }

    // Assigns the values from PlayerData to weekmoods and updates it.
    private void ValuesToWeekMoods()
    {
        for (int i = 0; i < _weekMoods.Length; i++)
        {
            _weekMoods[i].GetComponent<Image>().sprite = _moodImages[Convert.ToInt32(_playerData.playerDataMoodList[i])];
        }
    }
}
