using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
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
