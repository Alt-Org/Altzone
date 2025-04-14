using System;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class WeekEmotions : AltMonoBehaviour
{
    // Sprites that are used to show the moods in profile.
    [SerializeField] private Sprite[] _emotionImages;
    [SerializeField] private Sprite _blankEmotionImage;

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

        ValuesToWeekEmotions();
    }

    // Assigns the values from PlayerData to weekmoods and updates it.
    private void ValuesToWeekEmotions()
    {
        if (!string.IsNullOrWhiteSpace(_playerData.emotionSelectorDate))
        {
            for (int i = 0; i < _weekEmotions.Length; i++)
            {
                _weekEmotions[i].GetComponent<Image>().sprite = _blankEmotionImage;
            }
        }

        StartCoroutine(SavePlayerData(_playerData, null));

        for (int i = 0; i < _weekEmotions.Length; i++)
        {
            if ((int)_playerData.playerDataEmotionList[i] == -1) _weekEmotions[i].GetComponent<Image>().sprite = _blankEmotionImage;
            else
             _weekEmotions[i].GetComponent<Image>().sprite = _emotionImages[(int)_playerData.playerDataEmotionList[i]];
        }
    }
}
