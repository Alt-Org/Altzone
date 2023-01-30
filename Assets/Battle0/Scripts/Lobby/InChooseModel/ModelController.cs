using System;
using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle0.Scripts.Lobby.InChooseModel
{
    /// <summary>
    /// UI controller for <c>CharacterModel</c> view.
    /// </summary>
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view;

        private IEnumerator Start()
        {
            Debug.Log("Start");
            yield return new WaitUntil(() => _view.IsReady);
            _view.Reset();
            _view.Title = $"Choose your character\r\nfor {Application.productName} {PhotonLobby.GameVersion}";
            var playerDataCache = GameConfig.Get().PlayerSettings;
            _view.PlayerName = playerDataCache.PlayerName;
            _view.ContinueButtonOnClick = ContinueButtonOnClick;
            var currentCharacterId = playerDataCache.CustomCharacterModelId;
            var characters = Storefront.Get().GetAllBattleCharacters();
            characters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            _view.SetCharacters(characters, currentCharacterId);
        }

        private void ContinueButtonOnClick()
        {
            Debug.Log("click");
            // Save player settings if changed before continuing!
            var playerDataCache = GameConfig.Get().PlayerSettings;
            if (_view.PlayerName != playerDataCache.PlayerName)
            {
                playerDataCache.SetPlayerName(_view.PlayerName);
            }
            if (_view.CurrentCharacterId != playerDataCache.CustomCharacterModelId)
            {
                playerDataCache.SetCustomCharacterModelId(_view.CurrentCharacterId);
            }
            if (PhotonNetwork.NickName != playerDataCache.PlayerName)
            {
                // Fix player name if it has been changed.
                PhotonNetwork.NickName = playerDataCache.PlayerName;
            }
        }
    }
}