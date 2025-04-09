using System;
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
        for (int i = 0; i < _weekEmotions.Length; i++)
        {
            _weekEmotions[i].GetComponent<Image>().sprite = _emotionImages[Convert.ToInt32(_playerData.playerDataEmotionList[i])];
        }
    }
}
