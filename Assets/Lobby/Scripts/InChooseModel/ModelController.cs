using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InChooseModel
{
    /// <summary>
    /// UI controller for model view.
    /// </summary>
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view;

        private int _currentCharacterId;

        private void Start()
        {
            Debug.Log("Start");
            _view.titleText.text = $"Choose your character\r\nfor {Application.productName} {PhotonLobby.GameVersion}";
            var player = RuntimeGameConfig.Get().PlayerDataCache;
            _view.playerName.text = player.PlayerName;
            _view.hideCharacter();
            _view.continueButton.onClick.AddListener(() =>
            {
                Debug.Log("continueButton");
                // Save player settings if changed before continuing!
                if (_view.playerName.text != player.PlayerName ||
                    _currentCharacterId != player.CharacterModelId)
                {
                    Debug.Log("player.BatchSave");
                    player.BatchSave(() =>
                    {
                        player.PlayerName = _view.playerName.text;
                        player.CharacterModelId = _currentCharacterId;
                    });
                }
                if (PhotonNetwork.NickName != player.PlayerName)
                {
                    // Fix player name if it has been changed.
                    PhotonNetwork.NickName = player.PlayerName;
                }
            });
            _currentCharacterId = player.CharacterModelId;
            var characters = Storefront.Get().GetAllCharacterModels();
            characters.Sort((a, b) => string.Compare(a.sortValue(), b.sortValue(), StringComparison.Ordinal));
            for (var i = 0; i < characters.Count; ++i)
            {
                var character = characters[i];
                var button = _view.getButton(i);
                button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    _currentCharacterId = character.Id;
                    _view.showCharacter(character);
                });
                if (_currentCharacterId == character.Id)
                {
                    _view.showCharacter(character);
                }
            }
        }
    }
}