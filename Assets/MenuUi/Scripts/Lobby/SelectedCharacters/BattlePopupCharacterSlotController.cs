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


        private void OnEnable()
        {
            if (!_isInRoom) SetCharacters();
        }


        /// <summary>
        /// Set player characters. Gets info from player data.
        /// </summary>
        public void SetCharacters(bool editable = true)
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                var characters = playerData.CustomCharacters.ToList();
                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    CharacterID charID = playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == playerData.SelectedCharacterIds[i]) == null ? CharacterID.None : playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == playerData.SelectedCharacterIds[i]).Id;

                    if (charID is CharacterID.None)
                    {
                        _selectedCharacterSlots[i].SetEmpty(editable);
                        continue;
                    }

                    PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(((int)charID).ToString());
                    _selectedCharacterSlots[i].SetInfo(charInfo.GalleryImage, charID, editable);
                }
            }));
        }


        /// <summary>
        /// Set player characters based on given selected character ids. Stats are passed onwards to initialize piechart preview.
        /// </summary>
        /// <param name="selectedCharacterIds">The selected character ids to display.</param>
        /// <param name="stats">The stats for all three characters in an int array. Order: Hp, Speed, CharacterSize, Attack, Defence.</param>
        public void SetCharacters(int[] selectedCharacterIds, int[] stats)
        {
            for (int i = 0; i < selectedCharacterIds.Length; i++)
            {
                if (selectedCharacterIds[i] == 0)
                {
                    _selectedCharacterSlots[i].SetEmpty(false);
                    continue;
                }

                PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(selectedCharacterIds[i].ToString());
                _selectedCharacterSlots[i].SetInfo(charInfo.GalleryImage, charInfo.CharacterId, false, stats[(i * 5)..(i * 5 + 5)]);
            }
        }
    }
}
