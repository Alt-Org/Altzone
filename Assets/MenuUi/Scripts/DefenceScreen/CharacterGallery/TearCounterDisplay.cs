using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using MenuUI.Scripts;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Displays and updates the player's current tear (StatsDiamond) amount in DefenceView.
    /// </summary>
    public class TearCounterDisplay : AltMonoBehaviour
    {
        [SerializeField] private TMP_Text _tearAmountText;
        [SerializeField] private StatsWindowController _statsWindowController;

        private void OnEnable()
        {
            if (_statsWindowController == null)
            {
                // includeInactive: true, because StatsWindowController might be disabled initially
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
                if (playerData == null || _tearAmountText == null) return;

                _tearAmountText.text = playerData.DiamondSpeed.ToString();
            }));
        }
    }
}
