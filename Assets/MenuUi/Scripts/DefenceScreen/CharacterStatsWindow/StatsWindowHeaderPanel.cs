using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatsWindowHeaderPanel : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private TMP_Text _characterName;
        [SerializeField] private TMP_Text _diamondsAmount;
        [SerializeField] private TMP_Text _eraserAmount;

        private void OnEnable()
        {
            SetCharacterName();
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


        private void SetCharacterName()
        {
            if (_controller != null)
            {
                _characterName.text = _controller.GetCurrentCharacterName();
            }
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
