using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private TMP_Text _diamondsAmount;
        [SerializeField] private TMP_Text _eraserAmount;

        private void OnEnable()
        {
            SetDiamondsAmount();
            SetEraserAmount();

            _controller.OnDiamondAmountChanged += SetDiamondsAmount;
            _controller.OnEraserAmountChanged += SetEraserAmount;
        }


        private void OnDisable()
        {
            _controller.OnDiamondAmountChanged -= SetDiamondsAmount;
            _controller.OnEraserAmountChanged -= SetEraserAmount;
        }


        private void SetDiamondsAmount()
        {
            _diamondsAmount.text = _controller.UnlimitedDiamonds ? "rajaton" : _controller.GetDiamondAmount().ToString();
        }


        private void SetEraserAmount()
        {
            _eraserAmount.text = _controller.UnlimitedErasers ? "rajaton" : _controller.GetEraserAmount().ToString();
        }
    }
}
