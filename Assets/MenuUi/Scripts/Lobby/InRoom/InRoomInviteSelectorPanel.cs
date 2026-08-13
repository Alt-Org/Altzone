using System;
using System.Collections.Generic;
using System.Linq;
using Prg.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Dedicated panel for selecting which online player to invite into a Friend Lobby premade room.
    /// Assigned from prefab.
    /// </summary>
    public class InRoomInviteSelectorPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _emptyText;
        [SerializeField] private Image _overlayImage;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _scrollBackgroundImage;
        [SerializeField] private MatchInviteCandidateHandler _inviteItemPrefab;

        private readonly List<GameObject> _spawnedRows = new();
        private Action<ServerOnlinePlayer> _onSelected;
        private Action _onCancelled;

        private bool _closing;

        public bool IsVisible => _root != null && _root.activeSelf;

        private void Awake()
        {
            if (_root == null)
            {
                InitializePanel();
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnClosePressed);
            }
        }

        private void LateUpdate()
        {
            if (!IsVisible)
            {
                _closing = false;
                return;
            }

            if (ClickStateHandler.GetClickState() is ClickState.Start)
            {
                if (!IsPointerOnSelectorCard())
                {
                    _closing = true;
                }
            }

            if (ClickStateHandler.GetClickState() is ClickState.End && _closing)
            {
                if (!IsPointerOnSelectorCard())
                {
                    Hide(true);
                }
                _closing = false;
            }
        }

        public void Show(List<ServerOnlinePlayer> players, Action<ServerOnlinePlayer> onSelected, Action onCancelled = null)
        {
            if (_root == null)
            {
                InitializePanel();
            }

            if (_contentRoot == null)
            {
                Debug.LogWarning("InRoomInviteSelectorPanel: missing UI references.");
                return;
            }

            _onSelected = onSelected;
            _onCancelled = onCancelled;

            if (_titleText != null)
            {
                _titleText.text = "Valitse kutsuttava online-pelaaja";
            }

            BuildPlayerRows(players);
            _root.SetActive(true);
        }

        public void Hide(bool invokeCancel)
        {
            if (_root == null)
            {
                return;
            }

            bool wasVisible = _root.activeSelf;
            _root.SetActive(false);
            ClearRows();

            Action onCancelled = _onCancelled;
            _onSelected = null;
            _onCancelled = null;

            if (invokeCancel && wasVisible)
            {
                onCancelled?.Invoke();
            }
        }

        public void HideSilently()
        {
            Hide(false);
        }

        private void InitializePanel()
        {
            _root = gameObject;

            if (_closeButton == null)
            {
                return;
            }

            _closeButton.onClick.RemoveListener(OnClosePressed);
            _closeButton.onClick.AddListener(OnClosePressed);
        }

        private void OnClosePressed()
        {
            Hide(true);
        }

        private void OnPlayerPressed(ServerOnlinePlayer player)
        {
            Action<ServerOnlinePlayer> onSelected = _onSelected;
            Hide(false);
            onSelected?.Invoke(player);
        }

        private void BuildPlayerRows(List<ServerOnlinePlayer> players)
        {
            ClearRows();

            int candidateCount = players?.Count ?? 0;
            if (_emptyText != null)
            {
                _emptyText.gameObject.SetActive(candidateCount == 0);
                _emptyText.text = "Ei kutsuttavia online-pelaajia.";
            }

            if (candidateCount == 0)
            {
                return;
            }

            foreach (ServerOnlinePlayer player in players
                         .Where(player => player != null)
                         .OrderBy(GetDisplayName, StringComparer.OrdinalIgnoreCase))
            {
                GameObject row = CreateRowObject(_contentRoot, player);
                _spawnedRows.Add(row);
            }
        }

        private void ClearRows()
        {
            for (int i = 0; i < _spawnedRows.Count; i++)
            {
                if (_spawnedRows[i] != null)
                {
                    Destroy(_spawnedRows[i]);
                }
            }
            _spawnedRows.Clear();
        }

        private static string GetDisplayName(ServerOnlinePlayer player)
        {
            if (player == null)
            {
                return "Tuntematon";
            }

            if (!string.IsNullOrWhiteSpace(player.name))
            {
                return player.name;
            }

            return string.IsNullOrEmpty(player._id) ? "Tuntematon" : player._id;
        }

        private bool IsPointerOnSelectorCard()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            List<RaycastResult> results = new();
            PointerEventData data = new(EventSystem.current)
            {
                position = ClickStateHandler.GetClickPosition()
            };

            if (data.position == Vector2.negativeInfinity)
            {
                return false;
            }

            var modules = RaycasterManager.GetRaycasters();
            foreach (var module in modules)
            {
                module.Raycast(data, results);
            }

            Transform cardRoot = _cardImage != null ? _cardImage.transform : _root != null ? _root.transform : null;
            if (cardRoot == null)
            {
                return false;
            }

            foreach (RaycastResult result in results)
            {
                if (result.gameObject != null && result.gameObject.transform.IsChildOf(cardRoot))
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject CreateRowObject(Transform parent, ServerOnlinePlayer player)
        {
            MatchInviteCandidateHandler item = Instantiate(_inviteItemPrefab, parent);
            item.SetData(player, (player) => OnPlayerPressed(player));

            return item.gameObject;
        }
    }
}
