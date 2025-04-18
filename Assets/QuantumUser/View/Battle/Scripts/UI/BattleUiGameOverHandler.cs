using UnityEngine;
using UnityEngine.UI;

namespace Battle.View.UI
{
    public class BattleUiGameOverHandler : MonoBehaviour
    {
        [SerializeField] private BattleUiController _controller;
        [SerializeField] private GameObject _view;
        [SerializeField] private Button _button;

        public bool IsVisible => _view.activeSelf;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        private void Awake()
        {
            _button.onClick.AddListener(_controller.GameViewController.UiInputOnExitGamePressed);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
