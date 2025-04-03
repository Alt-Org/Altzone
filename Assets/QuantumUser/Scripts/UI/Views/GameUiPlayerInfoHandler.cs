using Altzone.Scripts.BattleUi;
using Altzone.Scripts.ModelV2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class GameUiPlayerInfoHandler : MonoBehaviour
    {
        [Header("Player name")]
        [SerializeField] private TMP_Text _playerName;

        [Header("Character slot 1")]
        [SerializeField] private Button _characterSelectButton1;
        [SerializeField] private Image _characterImage1;
        [SerializeField] private Image _damageFill1;
        [SerializeField] private Image _shieldFill1;

        [Header("Character slot 2")]
        [SerializeField] private Button _characterSelectButton2;
        [SerializeField] private Image _characterImage2;
        [SerializeField] private Image _damageFill2;
        [SerializeField] private Image _shieldFill2;

        [Header("Character slot 3")]
        [SerializeField] private Button _characterSelectButton3;
        [SerializeField] private Image _characterImage3;
        [SerializeField] private Image _damageFill3;
        [SerializeField] private Image _shieldFill3;

        [Header("Movable UI")]
        public BattleUiElement MovableUiElement;

        private void OnDisable()
        {
            MovableUiElement.gameObject.SetActive(false);
        }

        private void SetCharacterIcon(int characterId, int slotIdx)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());

            if (info == null) return;

            switch (slotIdx)
            {
                case 0:
                    _characterImage1.sprite = info.GalleryImage;
                    break;
                case 1:
                    _characterImage2.sprite = info.GalleryImage;
                    break;
                case 2:
                    _characterImage3.sprite = info.GalleryImage;
                    break;
            }
        }

        public void SetInfo(string playerName, int[] characterIds)
        {
            _playerName.text = playerName;

            for (int i = 0; i < characterIds.Length; i++)
            {
                SetCharacterIcon(characterIds[i], i);
            }

            MovableUiElement.gameObject.SetActive(true);
        }
    }
}

