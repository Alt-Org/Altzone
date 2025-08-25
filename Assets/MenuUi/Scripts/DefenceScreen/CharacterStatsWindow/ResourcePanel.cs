using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class ResourcePanel : AltMonoBehaviour
    {
        [SerializeField] private TMP_Text _diamondsAmount;

        private StatsWindowController _controller;
        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>(true);
            SetDiamondsAmount();

            _controller.OnUpgradeMaterialAmountChanged += SetDiamondsAmount;
        }

        private void OnDisable()
        {
            _controller.OnUpgradeMaterialAmountChanged -= SetDiamondsAmount;
        }


        private void SetDiamondsAmount()
        {
            if (SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials)
            {
                _diamondsAmount.text = "rajaton";
                return;
            }

            StartCoroutine(GetPlayerData((playerData) =>
            {
                _diamondsAmount.text = playerData.DiamondSpeed.ToString();
            }));
        }
    }
}
