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

            _controller.OnDiamondDecreased += SetDiamondsAmount;
            _controller.OnEraserDecreased += SetEraserAmount;
        }


        private void OnDisable()
        {
            _controller.OnDiamondDecreased -= SetDiamondsAmount;
            _controller.OnEraserDecreased -= SetEraserAmount;
        }


        private void SetDiamondsAmount()
        {
            _diamondsAmount.text = "rajaton"; // _controller.GetDiamondAmount().ToString();
        }


        private void SetEraserAmount()
        {
            _eraserAmount.text = "rajaton"; // _controller.GetEraserAmount().ToString();
        }
    }
}
