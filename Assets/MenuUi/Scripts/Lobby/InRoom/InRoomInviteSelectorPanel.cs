using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Dedicated panel for selecting which online player to invite into an InRoom_ premade room.
    /// Can be assigned from prefab or created at runtime by code.
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

            EnsureFallbackStyleInitialized();
            WireUi();
            if (_root != null)
            {
                _root.SetActive(false);
            }
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
            if (styleSourceButton == null)
            {
                return;
            }

            EnsureFallbackStyleInitialized();

            Image primaryImage = styleSourceButton.targetGraphic as Image;
            if (primaryImage != null)
            {
                _rowSprite = primaryImage.sprite;
                _rowMaterial = primaryImage.material;
                _rowImageType = primaryImage.type;
                _rowColor = primaryImage.color;
            }

            _rowColorBlock = styleSourceButton.colors;

            TMP_Text sourceText = styleSourceButton.GetComponentInChildren<TMP_Text>(true);
            if (sourceText != null)
            {
                _rowFontAsset = sourceText.font;
                _rowTextColor = sourceText.color;
                _rowFontSize = Mathf.Max(18f, sourceText.fontSize);
            }

            Image decorativeFrameImage = FindDecorativeFrameImage(styleSourceButton, primaryImage);
            if (decorativeFrameImage != null)
            {
                ApplyImageStyle(_cardImage, decorativeFrameImage.sprite, decorativeFrameImage.material, decorativeFrameImage.type, new Color(1f, 1f, 1f, 0.97f));
                ApplyImageStyle(_scrollBackgroundImage, decorativeFrameImage.sprite, decorativeFrameImage.material, decorativeFrameImage.type, new Color(1f, 1f, 1f, 0.9f));
            }

            if (_overlayImage != null)
            {
                _overlayImage.color = _fallbackOverlayColor;
            }

            if (_closeButtonImage == null && _closeButton != null)
            {
                _closeButtonImage = _closeButton.targetGraphic as Image;
            }

            if (_closeButtonImage != null)
            {
                ApplyImageStyle(_closeButtonImage, _rowSprite, _rowMaterial, _rowImageType, _rowColor);
            }

            if (_closeButton != null)
            {
                _closeButton.colors = _rowColorBlock;
            }

            if (_closeButtonText == null && _closeButton != null)
            {
                _closeButtonText = _closeButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (_closeButtonText != null)
            {
                _closeButtonText.color = _rowTextColor;
                if (_rowFontAsset != null) _closeButtonText.font = _rowFontAsset;
                _closeButtonText.fontSize = _rowFontSize;
            }

            if (_titleText != null)
            {
                _titleText.color = _rowTextColor;
                if (_rowFontAsset != null) _titleText.font = _rowFontAsset;
            }

            if (_emptyText != null)
            {
                _emptyText.color = _rowTextColor;
                if (_rowFontAsset != null) _emptyText.font = _rowFontAsset;
            }
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

        public static InRoomInviteSelectorPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("InviteSelectorPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image rootImage = root.GetComponent<Image>();
            rootImage.color = new Color(0f, 0f, 0f, 0.62f);

            GameObject card = new("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            card.transform.SetParent(root.transform, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.08f, 0.08f);
            cardRect.anchorMax = new Vector2(0.92f, 0.92f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(1f, 1f, 1f, 0.98f);

            VerticalLayoutGroup cardLayout = card.GetComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(24, 24, 22, 22);
            cardLayout.spacing = 14f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlHeight = true;
            cardLayout.childControlWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childForceExpandWidth = true;

            TMP_Text title = CreateHeaderText(card.transform, "Valitse kutsuttava online-pelaaja", 32f, TextAlignmentOptions.Center);
            AddLayoutElement(title.gameObject, 72f, 0f, false);

            GameObject scrollRoot = new("ScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
            scrollRoot.transform.SetParent(card.transform, false);
            Image scrollImage = scrollRoot.GetComponent<Image>();
            scrollImage.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);

            LayoutElement scrollLayout = scrollRoot.GetComponent<LayoutElement>();
            scrollLayout.minHeight = 260f;
            scrollLayout.flexibleHeight = 1f;

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.03f);
            Mask viewportMask = viewport.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(8, 8, 8, 8);

            ContentSizeFitter contentSizeFitter = content.GetComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollRoot.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 28f;

            TMP_Text emptyText = CreateHeaderText(card.transform, "Ei kutsuttavia online-pelaajia.", 24f, TextAlignmentOptions.Center);
            AddLayoutElement(emptyText.gameObject, 58f, 0f, false);

            GameObject closeButtonObject = new("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            closeButtonObject.transform.SetParent(card.transform, false);
            Image closeImage = closeButtonObject.GetComponent<Image>();
            closeImage.color = new Color(0.841f, 0.635f, 0.973f, 1f);

            LayoutElement closeLayout = closeButtonObject.GetComponent<LayoutElement>();
            closeLayout.preferredHeight = 70f;

            TMP_Text closeText = CreateHeaderText(closeButtonObject.transform, "Peruuta", 28f, TextAlignmentOptions.Center);
            RectTransform closeTextRect = closeText.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            InRoomInviteSelectorPanel panel = root.AddComponent<InRoomInviteSelectorPanel>();
            panel._root = root;
            panel._contentRoot = contentRect;
            panel._closeButton = closeButtonObject.GetComponent<Button>();
            panel._titleText = title;
            panel._emptyText = emptyText;
            panel._overlayImage = rootImage;
            panel._cardImage = cardImage;
            panel._scrollBackgroundImage = scrollImage;
            panel._closeButtonImage = closeImage;
            panel._closeButtonText = closeText;
            panel.EnsureFallbackStyleInitialized();
            panel.WireUi();
            panel.HideSilently();

            return panel;
        }

        private static TMP_Text CreateHeaderText(Transform parent, string textValue, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 0f);
            textRect.offsetMax = new Vector2(-6f, 0f);

            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = textValue;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.196f, 0.196f, 0.196f, 1f);
            text.enableWordWrapping = true;
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            return text;
        }

        private static void AddLayoutElement(GameObject gameObject, float preferredHeight, float flexibleHeight, bool flexibleWidth)
        {
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = flexibleHeight;
            layoutElement.flexibleWidth = flexibleWidth ? 1f : 0f;
        }

        private void EnsureFallbackStyleInitialized()
        {
            if (_rowStyleInitialized)
            {
                return;
            }

            _rowSprite = null;
            _rowMaterial = null;
            _rowImageType = Image.Type.Simple;
            _rowColor = _fallbackButtonColor;
            _rowTextColor = _fallbackTextColor;
            _rowFontSize = _fallbackRowFontSize;
            _rowFontAsset = TMP_Settings.defaultFontAsset;

            _rowColorBlock = ColorBlock.defaultColorBlock;
            _rowColorBlock.normalColor = Color.white;
            _rowColorBlock.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            _rowColorBlock.pressedColor = new Color(0.784f, 0.784f, 0.784f, 1f);
            _rowColorBlock.selectedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            _rowColorBlock.disabledColor = new Color(0.784f, 0.784f, 0.784f, 0.5f);
            _rowColorBlock.colorMultiplier = 1f;
            _rowColorBlock.fadeDuration = 0.1f;

            if (_overlayImage != null)
            {
                _overlayImage.color = _fallbackOverlayColor;
            }

            if (_cardImage != null)
            {
                _cardImage.color = _fallbackCardColor;
            }

            if (_scrollBackgroundImage != null)
            {
                _scrollBackgroundImage.color = _fallbackScrollColor;
            }

            if (_closeButtonImage != null)
            {
                _closeButtonImage.color = _fallbackButtonColor;
            }

            if (_closeButtonText != null)
            {
                _closeButtonText.color = _fallbackTextColor;
            }

            if (_titleText != null)
            {
                _titleText.color = _fallbackTextColor;
            }

            if (_emptyText != null)
            {
                _emptyText.color = _fallbackTextColor;
            }

            _rowStyleInitialized = true;
        }

        private static Image FindDecorativeFrameImage(Button sourceButton, Image primaryImage)
        {
            if (sourceButton == null)
            {
                return null;
            }

            Image[] images = sourceButton.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image == null || image == primaryImage) continue;
                if (image.sprite == null) continue;
                return image;
            }

            return primaryImage != null && primaryImage.sprite != null ? primaryImage : null;
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
