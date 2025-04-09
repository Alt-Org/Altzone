using UnityEngine;
using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles setting collected diamond amount text.
    /// </summary>
    public class BattleUiDiamondsHandler : MonoBehaviour
    {
        [SerializeField] private BattleUiMovableElement _movableUiElement;
        [SerializeField] private TMP_Text _diamondText;

        public bool IsVisible => MovableUiElement.gameObject.activeSelf;
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        public void SetDiamondsText(int diamondAmount)
        {
            _diamondText.text = diamondAmount.ToString();
        }
    }
}
