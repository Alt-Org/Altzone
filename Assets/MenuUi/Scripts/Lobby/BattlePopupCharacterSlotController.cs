using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;

public class BattlePopupCharacterSlotController : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup _layout;
    [SerializeField] private Transform _horizontalContentPanel;
    private CharacterSlot[] _curSelectedCharacterSlots;
    [SerializeField] private GameObject _characterSlotprefab;
    [SerializeField] private GalleryCharacterReference _referenceSheet;


    void Awake()
    {
        _curSelectedCharacterSlots = _horizontalContentPanel.GetComponentsInChildren<CharacterSlot>();
    }

    private void OnEnable()
    {
        float size = GetComponent<RectTransform>().rect.height - 20;
        Debug.LogWarning(size);
        _layout.cellSize = new(size, size);

        for (int i = 0; i < _curSelectedCharacterSlots.Length; i++)
        {
            _curSelectedCharacterSlots[i].GetComponent<GridLayoutGroup>().cellSize = new(size, size);
        }

        SetCharacters();
    }

    private void SetCharacters()
    {
        var gameConfig = GameConfig.Get();
        var playerSettings = gameConfig.PlayerSettings;
        var playerGuid = playerSettings.PlayerGuid;
        var store = Storefront.Get();
        store.GetPlayerData(playerGuid, playerData =>
        {
            //_playerData = playerData;
            //_view.OnCurrentCharacterIdChanged += HandleCurrentCharacterIdChanged;
            var currentCharacterIds = playerData.SelectedCharacterIds;
            var characters = playerData.SelectedCharacterIds;
            // Set characters in the ModelView
            //_view.SetCharacters(characters, currentCharacterId);

            for (int i = 0; i < _curSelectedCharacterSlots.Length; i++)
            {
                GalleryCharacterInfo info = _referenceSheet.GetCharacterPrefabInfo((int)characters[i]);

                if (info == null) continue;

                GameObject slot = Instantiate(_characterSlotprefab, _curSelectedCharacterSlots[i].transform);

                slot.GetComponent<CharacterSlot>().SetInfo(info.Image, info.Name, (CharacterID)characters[i], null);

                slot.transform.Find("Button").gameObject.SetActive(false);

                slot.GetComponent<Button>().enabled = false;

            }

        });
    }

}