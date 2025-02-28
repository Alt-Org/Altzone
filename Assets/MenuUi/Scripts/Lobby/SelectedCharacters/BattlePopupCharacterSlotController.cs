using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    public class BattlePopupCharacterSlotController : AltMonoBehaviour
    {
        [SerializeField] private BattlePopupSelectedCharacter[] _selectedCharacterSlots;

        private void OnEnable()
        {
            SetCharacters();
        }


        private void Start()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
        }


        private void SetCharacters()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                var characters = playerData.CustomCharacters.ToList();
                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    CharacterID charID = playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == playerData.SelectedCharacterIds[i]) == null ? CharacterID.None : playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == playerData.SelectedCharacterIds[i]).Id;
                    PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(((int)charID).ToString());
                    if (charID is CharacterID.None) continue;
                    _selectedCharacterSlots[i].SetInfo(charInfo.GalleryImage, charID, true);
                }
            }));
        }
    }
}
