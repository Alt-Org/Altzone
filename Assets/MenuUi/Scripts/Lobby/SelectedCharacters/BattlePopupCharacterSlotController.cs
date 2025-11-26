using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using MenuUi.Scripts.Signals;
using UnityEngine;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    /// <summary>
    /// Added to the SelectedCharacters prefab in Battle Popup. Handles forwarding the selected characters info to the 3 BattlePopupSelectedCharacter scripts which are connected to the slots.
    /// </summary>
    public class BattlePopupCharacterSlotController : AltMonoBehaviour
    {
        [SerializeField] private BattlePopupSelectedCharacter[] _selectedCharacterSlots;
        [SerializeField] private bool _isInRoom;
        [SerializeField] private Sprite _dragAndDropIcon;


        private void Awake()
        {
            if (_isInRoom)
            {
                foreach (BattlePopupSelectedCharacter slot in _selectedCharacterSlots)
                {
                    //slot.SetOpenEditPanelListener();
                    //slot._cornerIcon.overrideSprite = _dragAndDropIcon;
                    //slot._resistanceIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                SignalBus.OnReloadCharacterGalleryRequested += SetCharacters;
            }
        }


        private void OnEnable()
        {
            if (!_isInRoom) SetCharacters();
        }


        private void OnDestroy()
        {
            if (!_isInRoom) SignalBus.OnReloadCharacterGalleryRequested -= SetCharacters;
        }


        /// <summary>
        /// Set player characters. Gets info from player data.
        /// </summary>
        public void SetCharacters()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    CharacterID charID;
                    if (playerData.SelectedCharacterIds.Length < i)
                    {
                        charID = CharacterID.None;
                    }
                    else
                    {
                        CustomCharacter matchingCharacter = playerData.CustomCharacters?.FirstOrDefault(x => x.ServerID == playerData.SelectedCharacterIds[i].ServerID);
                        charID = matchingCharacter == null || playerData.SelectedCharacterIds[i].CharacterID == CharacterID.None ? CharacterID.None : matchingCharacter.Id;
                    }

                    if (charID is CharacterID.None)
                    {
                        _selectedCharacterSlots[i].SetEmpty(true);
                        continue;
                    }

                    PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(((int)charID).ToString());
                    _selectedCharacterSlots[i].SetInfo(charInfo.GalleryHeadImage, charID, true);
                }
            }));
        }


        /// <summary>
        /// Set player characters based on given selected character ids. Stats are passed onwards to initialize piechart preview.
        /// </summary>
        /// <param name="selectedCharacterIds">The selected character ids to display.</param>
        /// <param name="stats">The stats for all three characters in an int array. Order: Hp, Speed, CharacterSize, Attack, Defence.</param>
        public void SetCharacters(int[] selectedCharacterIds, int[] stats = null)
        {
            for (int i = 0; i < Mathf.Min(selectedCharacterIds.Length,_selectedCharacterSlots.Length); i++)
            {
                if (selectedCharacterIds[i] == (int)CharacterID.None)
                {
                    _selectedCharacterSlots[i].SetEmpty(false);
                    continue;
                }

                PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(selectedCharacterIds[i].ToString());
                int[] statsForCharacter = stats != null ? stats[(i * 5)..(i * 5 + 5)] : null;
                _selectedCharacterSlots[i].SetInfo(charInfo.GalleryHeadImage, charInfo.CharacterId, false, statsForCharacter);
            }
        }

        /// <summary>
        /// Set slots to teh bot mode where there is just a blank head in the slot if no characters are given.
        /// </summary>
        /// <param name="selectedCharacterIds">The selected character ids to display.</param>
        public void SetBotCharacters(int[] selectedCharacterIds = null)
        {
            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                if (selectedCharacterIds == null || selectedCharacterIds.Length > i || selectedCharacterIds[i] == (int)CharacterID.None)
                {
                    _selectedCharacterSlots[i].SetBot();
                    continue;
                }

                PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(selectedCharacterIds[i].ToString());
                _selectedCharacterSlots[i].SetInfo(charInfo.GalleryHeadImage, charInfo.CharacterId, false);
            }
        }
    }
}
