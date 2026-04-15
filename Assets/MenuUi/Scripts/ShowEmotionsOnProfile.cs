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
    [SerializeField] private GameObject[] _todayHighlights;

    // Creates the variable that is used to get the list of the moods.
    private PlayerData _playerData;

    TimeSpan _days;
    int _blanks;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the needed playerdata
        //StartCoroutine(GetPlayerData(data => _playerData = data));

        //ValuesToWeekEmotions();
    }

    // Assigns the values from PlayerData to weekmoods and updates it.
    public void ValuesToWeekEmotions(PlayerData player)
    {
        _playerData = player;

        if (_playerData == null || _playerData.playerDataEmotionList == null)
        {
            ShowOtherPlayerEmotions();
            return;
        }

        // First fill all slots with blank images to clear any old data. This also handles the case where there are fewer than 7 emotions in the list.
        for (int i = 0; i < _weekEmotions.Length; i++)
        {
            if (_weekEmotions[i] == null)
            {
                //Debug.LogError($"Week emotion slot {i} is null on {gameObject.name}");
                continue;
            }

            _weekEmotions[i].GetComponent<Image>().sprite = _blankEmotionImage;
        }

        // If no date is set, we can't determine the order of the emotions, so we just show them in the order they are in the list.
        if (string.IsNullOrWhiteSpace(_playerData.emotionSelectorDate))
        {
            return;
        }

        DateTime anchorDate;
        if (!DateTime.TryParse(_playerData.emotionSelectorDate, out anchorDate))
        {
            //Debug.LogError("Could not parse emotionSelectorDate: " + _playerData.emotionSelectorDate);
            return;
        }

        // Changed to DayOfWeek form:
        // Ma=0, Ti=1, Ke=2, To=3, Pe=4, La=5, Su=6
        int anchorSlot = ((int)anchorDate.DayOfWeek + 6) % 7;

        for (int i = 0; i < _playerData.playerDataEmotionList.Count && i < 7; i++)
        {
            Emotion emotion = _playerData.playerDataEmotionList[i];

            // list[0] = anchorDate
            // list[1] = previous day
            // list[2] = day before yesterday etc.
            int targetSlot = (anchorSlot - i + 7) % 7;

            if (_weekEmotions[targetSlot] == null)
            {
                //Debug.LogError($"Week emotion target slot {targetSlot} is null on {gameObject.name}");
                continue;
            }

            Image image = _weekEmotions[targetSlot].GetComponent<Image>();
            if (image == null)
            {
                //Debug.LogError($"No Image component found on week emotion slot {targetSlot}");
                continue;
            }

            if (emotion == Emotion.Blank)
                image.sprite = _blankEmotionImage;
            else
                image.sprite = _emotionImages[(int)emotion];
        }

        UpdateTodayHighlight();
    }

    private void UpdateTodayHighlight()
    {
        if (_todayHighlights == null || _todayHighlights.Length == 0)
            return;

        int todaySlot = ((int)DateTime.Now.DayOfWeek + 6) % 7;

        for (int i = 0; i < _todayHighlights.Length; i++)
        {
            if (_todayHighlights[i] != null)
                _todayHighlights[i].SetActive(i == todaySlot);
        }
    }

    /// <summary>
    /// Shows the week emotions when viewing another player's profile.
    /// </summary>
    public void ShowOtherPlayerEmotions()
    {
        // Just fill with empty slots since no data can be fetched from the server yet.
        for (int i = 0; i < _weekEmotions.Length; i++)
        {
            _weekEmotions[i].GetComponent<Image>().sprite = _blankEmotionImage;
        }

        UpdateTodayHighlight();
    }
}
