using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using MenuUi.Scripts.AvatarEditor;
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
    private AvatarFaceLoader _player1Avatar;
    [SerializeField]
    private TMP_Text _player2Name;
    [SerializeField]
    private AvatarFaceLoader _player2Avatar;
    [SerializeField]
    private TMP_Text _player3Name;
    [SerializeField]
    private AvatarFaceLoader _player3Avatar;
    [SerializeField]
    private TMP_Text _player4Name;
    [SerializeField]
    private AvatarFaceLoader _player4Avatar;

    [Header("Single Player Panels"), SerializeField]
    private GameObject _teamAlpha1PlayerPanel;
    [SerializeField]
    private GameObject _teamBeta1PlayerPanel;
    [SerializeField]
    private TMP_Text _singlePlayer1Name;
    [SerializeField]
    private AvatarFaceLoader _singlePlayer1Avatar;
    [SerializeField]
    private TMP_Text _singlePlayer2Name;
    [SerializeField]
    private AvatarFaceLoader _singlePlayer2Avatar;

    [Header("Clan Panels"), SerializeField]
    private GameObject _teamAlphaPanel;
    [SerializeField]
    private GameObject _teamBetaPanel;

    [SerializeField]
    private TMP_Text _clanAlphaName;
    [SerializeField]
    private ClanHeartColorSetter _clanAlphaLogo;
    [SerializeField]
    private TMP_Text _clanBetaName;
    [SerializeField]
    private ClanHeartColorSetter _clanBetaLogo;

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
            StartCoroutine(FetchClanLogo(info.TeamAlphaId, c =>
            {
                _clanAlphaLogo.SetHeartColors(c);
            }));
            _clanBetaName.text = info.TeamBetaName;
            StartCoroutine(FetchClanLogo(info.TeamBetaId, c =>
            {
                _clanBetaLogo.SetHeartColors(c);
            }));
        }
        else
        {
            if (!string.IsNullOrEmpty(info.Player1Name) && !string.IsNullOrEmpty(info.Player2Name))
            {
                _teamAlpha2PlayerPanel.SetActive(true);
                _player1Name.text = info.Player1Name;
                StartCoroutine(FetchAvatarData(info.Player1Id, c =>
                {
                    _player1Avatar.UpdateVisuals(c);
                }));
                _player2Name.text = info.Player2Name;
                StartCoroutine(FetchAvatarData(info.Player2Id, c =>
                {
                    _player2Avatar.UpdateVisuals(c);
                }));
            }
            else if (!string.IsNullOrEmpty(info.Player1Name))
            {
                _teamAlpha1PlayerPanel.SetActive(true);
                _singlePlayer1Name.text = info.Player1Name;
                StartCoroutine(FetchAvatarData(info.Player1Id, c =>
                {
                    _singlePlayer1Avatar.UpdateVisuals(c);
                }));
            }
            else if (!string.IsNullOrEmpty(info.Player2Name))
            {
                _teamAlpha1PlayerPanel.SetActive(true);
                _singlePlayer1Name.text = info.Player2Name;
                StartCoroutine(FetchAvatarData(info.Player2Id, c =>
                {
                    _singlePlayer1Avatar.UpdateVisuals(c);
                }));
            }

            if (!string.IsNullOrEmpty(info.Player3Name) && !string.IsNullOrEmpty(info.Player4Name))
            {
                _teamBeta2PlayerPanel.SetActive(true);
                _player3Name.text = info.Player1Name;
                StartCoroutine(FetchAvatarData(info.Player3Id, c =>
                {
                    _player3Avatar.UpdateVisuals(c);
                }));
                _player4Name.text = info.Player2Name;
                StartCoroutine(FetchAvatarData(info.Player4Id, c =>
                {
                    _player4Avatar.UpdateVisuals(c);
                }));
            }
            else if (!string.IsNullOrEmpty(info.Player3Name))
            {
                _teamBeta1PlayerPanel.SetActive(true);
                _singlePlayer2Name.text = info.Player3Name;
                StartCoroutine(FetchAvatarData(info.Player3Id, c =>
                {
                    _singlePlayer2Avatar.UpdateVisuals(c);
                }));
            }
            else if (!string.IsNullOrEmpty(info.Player4Name))
            {
                _teamBeta1PlayerPanel.SetActive(true);
                _singlePlayer2Name.text = info.Player4Name;
                StartCoroutine(FetchAvatarData(info.Player4Id, c =>
                {
                    _singlePlayer2Avatar.UpdateVisuals(c);
                }));
            }
        }

    }

    private IEnumerator FetchAvatarData(string playerId, Action<AvatarVisualData> callback)
    {
        Debug.LogWarning(playerId);
        if (string.IsNullOrEmpty(playerId) || playerId == "Bot")
        {
            if (callback != null)
                callback(null);
            yield break;
        }

        AvatarVisualData avatarData = null;
        bool finished = false;

        if (playerId == ServerManager.Instance.Player._id)
        {
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, c =>
            {
                avatarData = AvatarDesignLoader.Instance.CreateAvatarVisualData(c.AvatarData);
            });
        }
        else
        {
            StartCoroutine(ServerManager.Instance.GetPlayerFromServer(playerId, c=>
            {
                avatarData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new PlayerData(c, true).AvatarData);
            }, f => finished = f));
            yield return new WaitUntil(() => avatarData != null || finished);
        }

        if (callback != null)
            callback(avatarData);
    }

    private IEnumerator FetchClanLogo(string clanId, Action<List<HeartPieceData>> callback)
    {
        if (string.IsNullOrEmpty(clanId)) yield break;

        List<HeartPieceData> clanLogo = null;
        ClanData data = null;
        bool finished = false;

        if (clanId == ServerManager.Instance.Clan._id)
        {
            Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, c =>
            {
                clanLogo = c.ClanHeartPieces;
            });
        }
        else
        {
            StartCoroutine(ServerManager.Instance.GetClanFromServer(clanId, c => data = new(c), f => finished = f));
            yield return new WaitUntil(() => data != null || finished);
            clanLogo = data.ClanHeartPieces;
        }

        if (callback != null)
            callback(clanLogo);
    }
}
