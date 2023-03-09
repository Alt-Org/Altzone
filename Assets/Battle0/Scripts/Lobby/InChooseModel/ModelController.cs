using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
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
            var gameConfig = GameConfig.Get();
            var playerDataModel = gameConfig.PlayerDataModel;
            _view.PlayerName = playerDataModel.Name;
            _view.ContinueButtonOnClick = ContinueButtonOnClick;
            var currentCharacterId = playerDataModel.CurrentCharacterModelId;
            var characters = Storefront.Get().GetAllBattleCharacters();
            characters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            _view.SetCharacters(characters, currentCharacterId);
        }

        private void ContinueButtonOnClick()
        {
            Debug.Log("click");
            var gameConfig = GameConfig.Get();
            var playerDataModel = gameConfig.PlayerDataModel;
            var currentCharacterId = playerDataModel.CurrentCharacterModelId;
            if (_view.CurrentCharacterId != currentCharacterId)
            {
                playerDataModel.CurrentCharacterModelId = _view.CurrentCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(playerDataModel);
            }
        }
    }
}