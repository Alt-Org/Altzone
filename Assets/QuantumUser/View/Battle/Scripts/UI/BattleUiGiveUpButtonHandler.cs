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
        [SerializeField] private BattleUiController _uiController;
        [SerializeField] private Button _giveUpButton;
        
        public BattleUiMovableElement MovableUiElement;

        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        private void Awake()
        {
            _giveUpButton.onClick.AddListener(_uiController.GameViewController.OnLocalPlayerGiveUp);
        }

        private void OnDestroy()
        {
            _giveUpButton.onClick.RemoveAllListeners();
        }
    }
}
