using System.Collections;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.CharacterGallery;
using UnityEngine;

public class BattlePopupCharacterSlotController : AltMonoBehaviour
{
    [SerializeField] private GalleryCharacter[] _selectedCharacterSlots;
    [SerializeField] private ClassColorReference _classColorReference;

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
                CharacterClassID charClassID = CustomCharacter.GetClassID(charID);
                _selectedCharacterSlots[i].SetInfo(charInfo.GalleryImage, _classColorReference.GetColor(charClassID), _classColorReference.GetAlternativeColor(charClassID), charInfo.Name, charID, null);
            }
        }));

        StartCoroutine(SetVisualsNextFrame());
    }

    private IEnumerator SetVisualsNextFrame() // coroutine for setting selectedvisuals for character because GalleryCharacter wasn't awake yet during SetCharacters() and piechart was not showing
    {
        yield return new WaitForEndOfFrame();
        foreach (GalleryCharacter character in _selectedCharacterSlots)
        {
            character.SetSelectedVisuals();
        }
    }
}
