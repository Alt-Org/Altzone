using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultHandler : MonoBehaviour
{
    [SerializeField]
    private Image _ownResultImage;

    [SerializeField]
    private GameObject _teamAlphaWingsImage;
    [SerializeField]
    private GameObject _teamBetaWingsImage;

    [SerializeField]
    private Sprite _winImageEN;
    [SerializeField]
    private Sprite _winImageFI;
    [SerializeField]
    private Sprite _loseImageEN;
    [SerializeField]
    private Sprite _loseImageFI;

    [SerializeField]
    private GameObject _teamAlphaPlayerPanel;
    [SerializeField]
    private GameObject _teamBetaPlayerPanel;

    [SerializeField]
    private TMP_Text _player1Name;
    [SerializeField]
    private TMP_Text _player2Name;
    [SerializeField]
    private TMP_Text _player3Name;
    [SerializeField]
    private TMP_Text _player4Name;

    [SerializeField]
    private GameObject _teamAlphaPanel;
    [SerializeField]
    private GameObject _teamBetaPanel;

    [SerializeField]
    private TMP_Text _clanAlphaName;
    [SerializeField]
    private TMP_Text _clanBetaName;

    public void SetBattleResult(bool result)
    {
        if (result)
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                _ownResultImage.sprite = _winImageFI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                _ownResultImage.sprite = _winImageEN;
            }
        }
        else
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                _ownResultImage.sprite = _loseImageFI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                _ownResultImage.sprite = _loseImageEN;
            }
        }

        _teamAlphaWingsImage.SetActive(false);
        _teamBetaWingsImage.SetActive(false);

        int winningTeam = DataCarrier.GetData<int>(DataCarrier.BattleWinner, clear: false, suppressWarning: true);



        if (winningTeam == 1)
        {
            _teamAlphaWingsImage.SetActive(true);
        }
        else if (winningTeam == 2)
        {
            _teamBetaWingsImage.SetActive(true);
        }

        BattleRoomInfo info = LobbyManager.Instance.RoomInfo;

        if(info.GameType is GameType.Clan2v2)
        {
            _teamAlphaPanel.SetActive(true);
            _teamBetaPanel.SetActive(true);
            _teamAlphaPlayerPanel.SetActive(false);
            _teamBetaPlayerPanel.SetActive(false);

            _clanAlphaName.text = info.TeamAlphaName;
            _clanBetaName.text = info.TeamBetaName;
        }
        else
        {
            _teamAlphaPanel.SetActive(false);
            _teamBetaPanel.SetActive(false);
            _teamAlphaPlayerPanel.SetActive(true);
            _teamBetaPlayerPanel.SetActive(true);

            _player1Name.text = info.Player1Name;
            _player2Name.text = info.Player2Name;
            _player3Name.text = info.Player3Name;
            _player4Name.text = info.Player4Name;
        }

    }
}
