using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ModelV2;
using TMPro;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform _characterGridContent;
        [SerializeField] private Transform _selectedGridContent;

        [SerializeField] private GameObject _characterSlotPrefab;

        [SerializeField] private ClassColorReference _classColorReference;

        private bool _isReady;

        // Array of character slots in selected grid
        private SelectedCharacterSlot[] _selectedCharacterSlots;
        // List of character slots in character grid
        private List<CharacterSlot> _characterSlots = new();

        public delegate void CharacterSelectedHandler(CharacterID characterId, int slotIdx);
        public event CharacterSelectedHandler OnCharacterSelected;

        public bool IsReady
        {
            get
            {
                return _isReady;
            }
        }


        private void Awake()
        {
            _selectedCharacterSlots = _selectedGridContent.GetComponentsInChildren<SelectedCharacterSlot>();
        }


        public void Reset()
        {
            _isReady = false;

            // Remove selected characters
            foreach (SelectedCharacterSlot slot in _selectedCharacterSlots)
            {
                GalleryCharacter topSlotCharacter = slot.transform.GetComponentInChildren<GalleryCharacter>();
                if (topSlotCharacter != null)
                {
                    Destroy(topSlotCharacter.gameObject);
                }
            }
            // Remove all character slots
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                if (!characterSlot.transform.IsChildOf(_selectedGridContent))
                    Destroy(characterSlot.gameObject);
            }
            _characterSlots.Clear();
            _isReady = true;
        }


        public void SetCharacters(List<CustomCharacter> characters, int[] currentCharacterIds)
        {
            DataStore store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (BaseCharacter character in allItems)
            {
                PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(((int)character.Id).ToString());
                if (info == null) continue;

                GameObject slot = Instantiate(_characterSlotPrefab, _characterGridContent);

                CharacterClassID classID = CustomCharacter.GetClassID(character.Id);
                Color bgColor = _classColorReference.GetColor(classID);
                Color bgAltColor = _classColorReference.GetAlternativeColor(classID);

                CharacterSlot charSlot = slot.GetComponent<CharacterSlot>();
                charSlot.SetInfo(info.GalleryImage, bgColor, bgAltColor, info.Name, character.Id);

                _characterSlots.Add(charSlot);

                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    if (character.Id == (CharacterID)currentCharacterIds[i])
                    {
                        charSlot.Character.transform.SetParent(_selectedCharacterSlots[i].transform);
                        charSlot.Character.SetSelectedVisuals();
                    }
                }
            }
        }


        private void SelectCharacter(CharacterID id, int selectedSlotIdx)
        {
            OnCharacterSelected?.Invoke(id, selectedSlotIdx);
        }


        /// <summary>
        /// Reorders selected characters to the left and saves it.
        /// </summary>
        public void ReorderSelectedCharacters()
        {
            List<GalleryCharacter> characters = new List<GalleryCharacter>();

            foreach (SelectedCharacterSlot slot in _selectedCharacterSlots) // if slot has character add its character to list
            {
                GalleryCharacter character = slot.transform.GetComponentInChildren<GalleryCharacter>();
                if (character != null)
                {
                    characters.Add(character);
                }
            }

            for (int i = 0; i < characters.Count; i++) // reparent the characters starting from the leftmost characterslot
            {
                characters[i].transform.SetParent(_selectedCharacterSlots[i].transform);
            }

            for (int i = 0; i < _selectedCharacterSlots.Length; i++) // save selected character ids
            {
                if (i < characters.Count)
                {
                    SelectCharacter(characters[i].Id, i);
                }
                else
                {
                    SelectCharacter(CharacterID.None, i);
                }
            }
        }
    }
}
