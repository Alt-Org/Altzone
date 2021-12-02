using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Lobby.Scripts.InChooseModel
{
    /// <summary>
    /// UI controller for model view.
    /// </summary>
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView view;

        [SerializeField] private int currentCharacterId;

        private void Start()
        {
            Debug.Log("Start");
            view.titleText.text = $"Choose your character\r\nfor {Application.productName} {PhotonLobby.gameVersion}";
            var player = RuntimeGameConfig.Get().PlayerDataCache;
            view.playerName.text = player.PlayerName;
            view.hideCharacter();
            view.continueButton.onClick.AddListener(() =>
            {
                Debug.Log("continueButton");
                // Save player settings if changed before continuing!
                if (view.playerName.text != player.PlayerName ||
                    currentCharacterId != player.CharacterModelId)
                {
                    Debug.Log("player.BatchSave");
                    player.BatchSave(() =>
                    {
                        player.PlayerName = view.playerName.text;
                        player.CharacterModelId = currentCharacterId;
                    });
                }
            });
            currentCharacterId = player.CharacterModelId;
            var characters = Storefront.Get().GetAllCharacterModels();
            characters.Sort((a, b) => string.Compare(a.sortValue(), b.sortValue(), StringComparison.Ordinal));
            for (var i = 0; i < characters.Count; ++i)
            {
                var character = characters[i];
                var button = view.getButton(i);
                button.SetCaption(character.Name);
                button.onClick.AddListener(() =>
                {
                    currentCharacterId = character.Id;
                    view.showCharacter(character);
                });
                if (currentCharacterId == character.Id)
                {
                    view.showCharacter(character);
                }
            }
        }
    }
}