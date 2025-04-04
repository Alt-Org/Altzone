using Altzone.Scripts.BattleUi;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui give up button functionality.
    /// </summary>
    public class GameUiGiveUpButtonHandler : MonoBehaviour
    {
        public Button GiveUpButton;
        public BattleUiElement MovableUiElement;

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

