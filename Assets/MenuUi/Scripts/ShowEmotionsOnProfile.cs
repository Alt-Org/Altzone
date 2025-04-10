using System;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class WeekEmotions : AltMonoBehaviour
{
    // Sprites that are used to show the moods in profile.
    [SerializeField] private Sprite[] _emotionImages;

    // Components that hold the sprite and shows the moods in profile.
    [SerializeField] private GameObject[] _weekEmotions;

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    TimeSpan _days;
    int _blanks;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the needed playerdata
        StartCoroutine(GetPlayerData(data => _playerData = data));

        _days = TimeSpan.Parse(_playerData.daysBetweenInput);

        // Sets the amount of blanks to be 6 if there are more than 7 days between input.
        if (Convert.ToInt32(_days.TotalDays) > 7)
        {
            _blanks = 6;
        } else
        {
            _blanks = Convert.ToInt32(_days.TotalDays);
        }

        ValuesToWeekEmotions();
    }

    // Assigns the values from PlayerData to weekmoods and updates it.
    private void ValuesToWeekEmotions()
    {
        // Sets values in list to be blank except the first one.
        for (int i = 1; i <= _blanks; i++)
        {
            _playerData._playerDataEmotionList[i] = Emotion.Blank.ToString();
        }

        StartCoroutine(SavePlayerData(_playerData, null));

        for (int i = 0; i < _weekEmotions.Length; i++)
        {
            _weekEmotions[i].GetComponent<Image>().sprite = _emotionImages[Convert.ToInt32(_playerData.playerDataEmotionList[i])];
        }
    }
}
