using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanSwipeButtonsToggler : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private BaseScrollRect _baseScrollRect;
    [SerializeField] private ClanMainView _clanMainView;

    [Tooltip("Kun horizontalNormalizedPosition ylitt�� t�m�n, tulkitaan ett� ollaan Members-sivulla.")]
    [Range(0f, 1f)]
    [SerializeField] private float _membersThreshold = 0.5f;

    private bool _lastOnProfile = true;

    private void OnEnable()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.AddListener(OnScrollChanged);

            UpdateFromScroll();
        }
        else if (_baseScrollRect != null)
        {
            _baseScrollRect.OnValueChanged.AddListener(OnScrollChanged);

            UpdateFromScroll();
        }
    }

    private void OnDisable()
    {
        if (_scrollRect != null)
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }
        else if (_baseScrollRect != null)
        {
            _baseScrollRect.OnValueChanged.RemoveListener(OnScrollChanged);
        }
    }

    private void OnScrollChanged(Vector2 _)
    {
        UpdateFromScroll();
    }

    private void UpdateFromScroll()
    {
        if (_scrollRect == null && _baseScrollRect == null || _clanMainView == null) return;

        float x;
        if (_scrollRect) x = _scrollRect.horizontalNormalizedPosition;
        else x = _baseScrollRect.HorizontalNormalizedPosition;
        bool onProfile = x < _membersThreshold;

        if (onProfile == _lastOnProfile) return;
        _lastOnProfile = onProfile;

        if (onProfile)
            _clanMainView.SetCurrentPageToProfile();
        else
            _clanMainView.SetCurrentPageToMembers();
    }
}
