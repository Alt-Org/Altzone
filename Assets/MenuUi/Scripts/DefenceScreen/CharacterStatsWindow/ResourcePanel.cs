using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _diamondsAmount;

        private StatsWindowController _controller;
        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();
            SetDiamondsAmount();

            _controller.OnUpgradeMaterialAmountChanged += SetDiamondsAmount;
        }


        private void OnDisable()
        {
            _controller.OnUpgradeMaterialAmountChanged -= SetDiamondsAmount;
        }


        private void SetDiamondsAmount()
        {
            _diamondsAmount.text = SettingsCarrier.Instance.UnlimitedStatUpgradeMaterials ? "rajaton" : _controller.GetUpgradeMaterialAmount().ToString();
        }
    }
}
