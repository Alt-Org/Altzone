using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;

public class ClanSwipeButtonsToggler : MonoBehaviour
{
    [SerializeField] private SwipeUI _swipeUI;
    [SerializeField] private ClanMainView _clanMainView;

    private void OnEnable()
    {
        if (_swipeUI != null)
        {
            _swipeUI.OnCurrentPageChanged += HandleCurrentPageChanged;
            HandleCurrentPageChanged();
        }
    }

    private void OnDisable()
    {
        if (_swipeUI != null)
        {
            _swipeUI.OnCurrentPageChanged -= HandleCurrentPageChanged;
        }
    }

    private void HandleCurrentPageChanged()
    {
        if (_swipeUI == null || _clanMainView == null)
            return;

        bool onProfilePage = _swipeUI.CurrentPage == 0;

        _clanMainView.SetCurrentPageFromSwipe(onProfilePage);
    }
}
