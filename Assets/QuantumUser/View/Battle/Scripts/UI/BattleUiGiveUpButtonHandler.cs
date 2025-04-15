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
        [SerializeField] private BattleUiMovableElement _movableUiElement;
        [SerializeField] private Button _giveUpButton;

        public bool IsVisible => MovableUiElement.gameObject.activeSelf;
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        private void Awake()
        {
            _giveUpButton.onClick.AddListener(_uiController.GameViewController.UiInputOnLocalPlayerGiveUp);
        }

        private void OnDestroy()
        {
            _giveUpButton.onClick.RemoveAllListeners();
        }
    }
}
