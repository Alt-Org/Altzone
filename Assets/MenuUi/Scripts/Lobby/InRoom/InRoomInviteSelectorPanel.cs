using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        [SerializeField] private Image _closeButtonImage;
        [SerializeField] private TMP_Text _closeButtonText;

        [Header("Fallback Style")]
        [SerializeField] private Color _fallbackOverlayColor = new(0f, 0f, 0f, 0.62f);
        [SerializeField] private Color _fallbackCardColor = new(1f, 1f, 1f, 0.98f);
        [SerializeField] private Color _fallbackScrollColor = new(0.95f, 0.95f, 0.95f, 0.95f);
        [SerializeField] private Color _fallbackButtonColor = new(0.841f, 0.635f, 0.973f, 1f);
        [SerializeField] private Color _fallbackTextColor = new(0.196f, 0.196f, 0.196f, 1f);
        [SerializeField] private float _fallbackRowFontSize = 24f;

        private readonly List<GameObject> _spawnedRows = new();
        private Action<ServerOnlinePlayer> _onSelected;
        private Action _onCancelled;

        private Sprite _rowSprite;
        private Material _rowMaterial;
        private Image.Type _rowImageType = Image.Type.Simple;
        private Color _rowColor;
        private ColorBlock _rowColorBlock;
        private TMP_FontAsset _rowFontAsset;
        private Color _rowTextColor;
        private float _rowFontSize;
        private bool _rowStyleInitialized;

        public bool IsVisible => _root != null && _root.activeSelf;

        private void Awake()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            WireUi();
            EnsureFallbackStyleInitialized();
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnClosePressed);
            }
        }

        public void Show(List<ServerOnlinePlayer> players, Action<ServerOnlinePlayer> onSelected, Action onCancelled = null)
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            // Show can be called before Awake when panel starts inactive in prefab.
            WireUi();
            EnsureFallbackStyleInitialized();

            if (_root == null || _contentRoot == null)
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

        public void ConfigureVisualStyle(Button styleSourceButton)
        {
            _ = styleSourceButton;
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

        private void WireUi()
        {
            if (_closeButton == null)
            {
                return;
            }

            if (_closeButtonImage == null)
            {
                _closeButtonImage = _closeButton.targetGraphic as Image;
            }

            if (_closeButtonText == null)
            {
                _closeButtonText = _closeButton.GetComponentInChildren<TMP_Text>(true);
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

            foreach (ServerOnlinePlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                GameObject row = CreateRowObject(_contentRoot, player);
                Button button = row.GetComponent<Button>();
                if (button != null)
                {
                    ServerOnlinePlayer selectedPlayer = player;
                    button.onClick.AddListener(() => OnPlayerPressed(selectedPlayer));
                }
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

        private GameObject CreateRowObject(Transform parent, ServerOnlinePlayer player)
        {
            GameObject row = new("InviteCandidateRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            Image image = row.GetComponent<Image>();
            ApplyImageStyle(image, _rowSprite, _rowMaterial, _rowImageType, _rowColor);

            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 82f;

            Button button = row.GetComponent<Button>();
            button.targetGraphic = image;
            button.colors = _rowColorBlock;

            GameObject textObj = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(row.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(22f, 8f);
            textRect.offsetMax = new Vector2(-22f, -8f);

            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            string displayName = GetDisplayName(player);
            string idText = player == null ? string.Empty : player._id;
            text.richText = true;
            text.text = string.IsNullOrEmpty(idText) || idText == displayName
                ? displayName
                : $"{displayName}\n<size=72%>{idText}</size>";
            text.fontSize = _rowFontSize;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = _rowTextColor;
            text.enableAutoSizing = true;
            text.fontSizeMin = 18f;
            text.fontSizeMax = Mathf.Max(20f, _rowFontSize);
            if (_rowFontAsset != null) text.font = _rowFontAsset;

            return row;
        }

        private void EnsureFallbackStyleInitialized()
        {
            if (_rowStyleInitialized)
            {
                return;
            }

            if (_closeButtonImage == null && _closeButton != null)
            {
                _closeButtonImage = _closeButton.targetGraphic as Image;
            }

            if (_closeButtonText == null && _closeButton != null)
            {
                _closeButtonText = _closeButton.GetComponentInChildren<TMP_Text>(true);
            }

            _rowSprite = _closeButtonImage != null ? _closeButtonImage.sprite : null;
            _rowMaterial = _closeButtonImage != null ? _closeButtonImage.material : null;
            _rowImageType = _closeButtonImage != null && _closeButtonImage.sprite != null ? _closeButtonImage.type : Image.Type.Simple;
            _rowColor = _closeButtonImage != null ? _closeButtonImage.color : _fallbackButtonColor;

            _rowColorBlock = _closeButton != null ? _closeButton.colors : ColorBlock.defaultColorBlock;

            _rowTextColor = _closeButtonText != null ? _closeButtonText.color : _fallbackTextColor;
            _rowFontSize = _closeButtonText != null ? Mathf.Max(18f, _closeButtonText.fontSize) : _fallbackRowFontSize;
            _rowFontAsset = _closeButtonText != null && _closeButtonText.font != null ? _closeButtonText.font : TMP_Settings.defaultFontAsset;

            _rowStyleInitialized = true;
        }

        private static void ApplyImageStyle(Image image, Sprite sprite, Material material, Image.Type type, Color color)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.material = material;
            image.type = sprite != null ? type : Image.Type.Simple;
            image.color = color;
        }

    }
}
