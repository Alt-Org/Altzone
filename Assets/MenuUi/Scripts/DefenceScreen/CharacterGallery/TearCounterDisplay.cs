using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using MenuUI.Scripts;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Displays and updates the player's current tear (StatsDiamond) amount across individual digit slots in DefenceView.
    /// </summary>
    public class TearCounterDisplay : AltMonoBehaviour
    {
        [SerializeField] private TMP_Text[] _digitTexts;
        [SerializeField] private StatsWindowController _statsWindowController;

        private void OnEnable()
        {
            if (_statsWindowController == null)
            {
                _statsWindowController = FindObjectOfType<StatsWindowController>(true);
            }

            if (_statsWindowController != null)
            {
                _statsWindowController.OnUpgradeMaterialAmountChanged += UpdateTearAmount;
            }

            UpdateTearAmount();
        }

        private void OnDisable()
        {
            if (_statsWindowController != null)
            {
                _statsWindowController.OnUpgradeMaterialAmountChanged -= UpdateTearAmount;
            }
        }

        public void UpdateTearAmount()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                if (playerData == null || _digitTexts == null || _digitTexts.Length == 0) return;

                string formatted = playerData.DiamondSpeed.ToString();

                if (formatted.Length > _digitTexts.Length)
                {
                    formatted = new string('9', _digitTexts.Length);
                }
                else
                {
                    formatted = formatted.PadLeft(_digitTexts.Length, ' ');
                }

                for (int i = 0; i < _digitTexts.Length; i++)
                {
                    if (_digitTexts[i] != null)
                    {
                        _digitTexts[i].text = formatted[i] == ' ' ? string.Empty : formatted[i].ToString();
                    }
                }
            }));
        }
    }
}
