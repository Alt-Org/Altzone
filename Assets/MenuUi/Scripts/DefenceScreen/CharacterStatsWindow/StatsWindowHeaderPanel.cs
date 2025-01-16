using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
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
            _diamondsAmount.text = _controller.GetDiamondAmount().ToString();
        }


        private void SetEraserAmount()
        {
            _eraserAmount.text = _controller.GetEraserAmount().ToString();
        }
    }
}
