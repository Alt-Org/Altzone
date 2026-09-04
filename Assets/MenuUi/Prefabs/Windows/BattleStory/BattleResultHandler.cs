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

    [Header("Result Wings Objects"), SerializeField]
    private GameObject _teamAlphaWingsImage;
    [SerializeField]
    private GameObject _teamBetaWingsImage;

    [Header("Result sprites"), SerializeField]
    private Sprite _winImageEN;
    [SerializeField]
    private Sprite _winImageFI;
    [SerializeField]
    private Sprite _loseImageEN;
    [SerializeField]
    private Sprite _loseImageFI;

    [Header("Two Player Panels"), SerializeField]
    private GameObject _teamAlpha2PlayerPanel;
    [SerializeField]
    private GameObject _teamBeta2PlayerPanel;
    [SerializeField]
    private TMP_Text _player1Name;
    [SerializeField]
    private TMP_Text _player2Name;
    [SerializeField]
    private TMP_Text _player3Name;
    [SerializeField]
    private TMP_Text _player4Name;

    [Header("Single Player Panels"), SerializeField]
    private GameObject _teamAlpha1PlayerPanel;
    [SerializeField]
    private GameObject _teamBeta1PlayerPanel;
    [SerializeField]
    private TMP_Text _singlePlayer1Name;
    [SerializeField]
    private TMP_Text _singlePlayer2Name;

    [Header("Clan Panels"), SerializeField]
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

        _teamAlphaPanel.SetActive(false);
        _teamBetaPanel.SetActive(false);
        _teamAlpha2PlayerPanel.SetActive(false);
        _teamBeta2PlayerPanel.SetActive(false);
        _teamAlpha1PlayerPanel.SetActive(false);
        _teamBeta1PlayerPanel.SetActive(false);

        if (info.GameType is GameType.Clan2v2)
        {
            _teamAlphaPanel.SetActive(true);
            _teamBetaPanel.SetActive(true);

            _clanAlphaName.text = info.TeamAlphaName;
            _clanBetaName.text = info.TeamBetaName;
        }
        else
        {
            if (!string.IsNullOrEmpty(info.Player1Name) && !string.IsNullOrEmpty(info.Player2Name))
            {
                _teamAlpha2PlayerPanel.SetActive(true);
                _player1Name.text = info.Player1Name;
                _player2Name.text = info.Player2Name;
            }
            else if (!string.IsNullOrEmpty(info.Player1Name))
            {
                _teamAlpha1PlayerPanel.SetActive(true);
                _singlePlayer1Name.text = info.Player1Name;
            }
            else if (!string.IsNullOrEmpty(info.Player2Name))
            {
                _teamAlpha1PlayerPanel.SetActive(true);
                _singlePlayer1Name.text = info.Player2Name;
            }

            if (!string.IsNullOrEmpty(info.Player3Name) && !string.IsNullOrEmpty(info.Player4Name))
            {
                _teamBeta2PlayerPanel.SetActive(true);
                _player1Name.text = info.Player1Name;
                _player2Name.text = info.Player2Name;
            }
            else if (!string.IsNullOrEmpty(info.Player3Name))
            {
                _teamBeta1PlayerPanel.SetActive(true);
                _singlePlayer2Name.text = info.Player3Name;
            }
            else if (!string.IsNullOrEmpty(info.Player4Name))
            {
                _teamBeta1PlayerPanel.SetActive(true);
                _singlePlayer2Name.text = info.Player4Name;
            }
        }

    }
}
