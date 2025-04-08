using Altzone.Scripts.BattleUiShared;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui give up button functionality.
    /// </summary>
    public class BattleUiGiveUpButtonHandler : MonoBehaviour
    {
        public Button GiveUpButton;
        public BattleUiMovableElement MovableUiElement;

        private void OnDestroy()
        {
            GiveUpButton.onClick.RemoveAllListeners();
        }

        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }
    }
}

