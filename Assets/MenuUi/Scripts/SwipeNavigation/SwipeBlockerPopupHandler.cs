using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class SwipeBlockerPopupHandler : MonoBehaviour
    {
        [SerializeField] private SwipeBlocker _blocker;

        private List<GameObject> _openPopupsList = new();

        private void OnEnable()
        {
            _openPopupsList.Clear();
            _blocker.gameObject.SetActive(false);
        }

        public void OpenPopup(GameObject popup)
        {
            if (popup == null) return;
            if (_openPopupsList.Contains(popup)) return;
            _openPopupsList.Add(popup);
            if (_blocker.gameObject.activeSelf) return;
            else _blocker.gameObject.SetActive(true);
        }

        public void ClosePopup(GameObject popup)
        {
            if (popup == null) return;
            if (!_openPopupsList.Contains(popup)) return;
            _openPopupsList.Remove(popup);
            if (_openPopupsList.Count == 0 && _blocker.gameObject.activeSelf) _blocker.gameObject.SetActive(false);
        }
    }
}
