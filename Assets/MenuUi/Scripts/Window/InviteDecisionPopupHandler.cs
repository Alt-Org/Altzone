using System;
using System.Linq;
using MenuUi.Scripts.Lobby.InLobby;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class InviteDecisionPopupHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [FormerlySerializedAs("_returnButton")]
        [SerializeField] private Button _rejectButton;
        [FormerlySerializedAs("_closeButton")]
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TMP_Text _messageText;
        [FormerlySerializedAs("_returnButtonText")]
        [SerializeField] private TMP_Text _rejectButtonText;
        [FormerlySerializedAs("_closeButtonText")]
        [SerializeField] private TMP_Text _acceptButtonText;
        [SerializeField] private string _defaultMessage = "Sinut kutsuttiin huoneeseen. Liitytäänkö?";
        [SerializeField] private string _defaultDeclineText = "Hylkää";
        [SerializeField] private string _defaultAcceptText = "Liity";

        private const string InvitePopupResourcePath = "InviteDecisionPanel";
        private const int RuntimeFallbackSortingOrder = 5000;

        private static InviteDecisionPopupHandler _instance;
        private static bool _isInstantiating;

        private bool _waitingResponse;
        private Action<bool> _decisionResponse;

        public static bool RequestInviteDecisionPrompt(string message, string acceptText, string declineText, Action<bool> responseCallback)
        {
            InviteDecisionPopupHandler handler = EnsureInstance();
            if (handler == null)
            {
                return false;
            }

            handler.OpenPopup(message, acceptText, declineText, responseCallback);
            return true;
        }

        private static InviteDecisionPopupHandler EnsureInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            InviteDecisionPopupHandler existingInstance = Resources.FindObjectsOfTypeAll<InviteDecisionPopupHandler>()
                .Where(handler => handler != null
                    && handler.gameObject != null
                    && handler.gameObject.scene.IsValid())
                .OrderByDescending(handler => handler.gameObject.activeInHierarchy)
                .FirstOrDefault();
            if (existingInstance != null)
            {
                _instance = existingInstance;
                return _instance;
            }

            if (_isInstantiating)
            {
                return null;
            }

            _isInstantiating = true;
            try
            {
                GameObject prefab = Resources.Load<GameObject>(InvitePopupResourcePath);
                if (prefab == null)
                {
                    Debug.LogWarning($"InviteDecisionPopupHandler: could not load Resources/{InvitePopupResourcePath}.prefab");
                    return null;
                }

                Transform parent = ResolvePreferredParent();
                bool useResolvedParent = IsUsableParent(parent);
                GameObject popupInstance = useResolvedParent
                    ? Instantiate(prefab, parent, false)
                    : Instantiate(prefab);

                popupInstance.name = "InviteDecisionPanel";
                if (!useResolvedParent)
                {
                    EnsureStandaloneCanvas(popupInstance);
                    DontDestroyOnLoad(popupInstance);
                }
                else
                {
                    popupInstance.transform.SetAsLastSibling();
                }

                InviteDecisionPopupHandler handler = popupInstance.GetComponent<InviteDecisionPopupHandler>();
                if (handler == null)
                {
                    Debug.LogWarning("InviteDecisionPopupHandler: prefab is missing InviteDecisionPopupHandler component.");
                    Destroy(popupInstance);
                    return null;
                }

                _instance = handler;
                return handler;
            }
            finally
            {
                _isInstantiating = false;
            }
        }

        private static Transform ResolvePreferredParent()
        {
            GameObject directUiOverlayPanel = GameObject.Find("UIOverlayPanel");
            if (directUiOverlayPanel != null
                && directUiOverlayPanel.scene.IsValid()
                && directUiOverlayPanel.activeInHierarchy)
            {
                return directUiOverlayPanel.transform;
            }

            GameObject overlayPanel = GameObject.FindWithTag("OverlayPanel");
            if (overlayPanel != null && overlayPanel.scene.IsValid() && overlayPanel.activeInHierarchy)
            {
                Transform overlayContent = overlayPanel.transform.Find("UIOverlayPanel");
                if (overlayContent != null
                    && overlayContent.gameObject.scene.IsValid()
                    && overlayContent.gameObject.activeInHierarchy)
                {
                    return overlayContent;
                }

                return overlayPanel.transform;
            }

            OverlayPanelCheck overlayPanelCheck = Resources.FindObjectsOfTypeAll<OverlayPanelCheck>()
                .FirstOrDefault(check => check != null
                    && check.gameObject != null
                    && check.gameObject.scene.IsValid()
                    && check.gameObject.activeInHierarchy);
            if (overlayPanelCheck != null)
            {
                Transform overlayContent = overlayPanelCheck.transform.Find("UIOverlayPanel");
                if (overlayContent != null
                    && overlayContent.gameObject.scene.IsValid()
                    && overlayContent.gameObject.activeInHierarchy)
                {
                    return overlayContent;
                }

                return overlayPanelCheck.transform;
            }

            if (InLobbyController.PopupContentsInstance != null
                && InLobbyController.PopupContentsInstance.scene.IsValid()
                && InLobbyController.PopupContentsInstance.activeInHierarchy)
            {
                return InLobbyController.PopupContentsInstance.transform;
            }

            Canvas fallbackCanvas = Resources.FindObjectsOfTypeAll<Canvas>()
                .Where(canvas => canvas != null
                    && canvas.gameObject != null
                    && canvas.gameObject.scene.IsValid()
                    && canvas.gameObject.activeInHierarchy)
                .OrderByDescending(canvas => canvas.sortingOrder)
                .FirstOrDefault();
            return fallbackCanvas != null ? fallbackCanvas.transform : null;
        }

        private static bool IsUsableParent(Transform parent)
        {
            return parent != null
                && parent.gameObject != null
                && parent.gameObject.scene.IsValid()
                && parent.gameObject.activeInHierarchy;
        }

        private static void EnsureStandaloneCanvas(GameObject popupRoot)
        {
            if (popupRoot == null) return;

            Canvas canvas = popupRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = popupRoot.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = RuntimeFallbackSortingOrder;

            if (popupRoot.GetComponent<GraphicRaycaster>() == null)
            {
                popupRoot.AddComponent<GraphicRaycaster>();
            }

            if (popupRoot.GetComponent<CanvasScaler>() == null)
            {
                popupRoot.AddComponent<CanvasScaler>();
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            if (_rejectButton != null)
            {
                _rejectButton.onClick.AddListener(OnDecline);
            }

            if (_acceptButton != null)
            {
                _acceptButton.onClick.AddListener(OnAccept);
            }
        }

        private void OnDisable()
        {
            if (_rejectButton != null)
            {
                _rejectButton.onClick.RemoveListener(OnDecline);
            }

            if (_acceptButton != null)
            {
                _acceptButton.onClick.RemoveListener(OnAccept);
            }

            if (_waitingResponse)
            {
                Resolve(false);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OpenPopup(string message, string acceptText, string declineText, Action<bool> responseCallback)
        {
            EnsureHostIsVisible();
            CacheTextReferences();

            if (_messageText != null)
            {
                _messageText.text = string.IsNullOrEmpty(message) ? _defaultMessage : message;
            }

            if (_acceptButtonText != null)
            {
                _acceptButtonText.text = string.IsNullOrEmpty(acceptText) ? _defaultAcceptText : acceptText;
            }

            if (_rejectButtonText != null)
            {
                _rejectButtonText.text = string.IsNullOrEmpty(declineText) ? _defaultDeclineText : declineText;
            }

            _decisionResponse = responseCallback;
            _waitingResponse = true;
            GameObject popupTarget = GetPopupTarget();
            popupTarget.SetActive(true);

            Transform targetTransform = popupTarget.transform;
            targetTransform.SetAsLastSibling();

            Canvas rootCanvas = targetTransform.root != null ? targetTransform.root.GetComponent<Canvas>() : null;
            if (rootCanvas != null && rootCanvas.overrideSorting)
            {
                rootCanvas.sortingOrder = Mathf.Max(rootCanvas.sortingOrder, RuntimeFallbackSortingOrder);
            }
        }

        private void EnsureHostIsVisible()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (gameObject.activeInHierarchy)
            {
                return;
            }

            Transform preferredParent = ResolvePreferredParent();
            if (IsUsableParent(preferredParent))
            {
                transform.SetParent(preferredParent, false);
                transform.SetAsLastSibling();
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
                return;
            }

            transform.SetParent(null, false);
            EnsureStandaloneCanvas(gameObject);
            DontDestroyOnLoad(gameObject);
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        private void OnAccept()
        {
            Resolve(true);
        }

        private void OnDecline()
        {
            Resolve(false);
        }

        private void Resolve(bool accepted)
        {
            _waitingResponse = false;
            GetPopupTarget().SetActive(false);

            Action<bool> response = _decisionResponse;
            _decisionResponse = null;
            response?.Invoke(accepted);
        }

        private GameObject GetPopupTarget()
        {
            return _popup != null ? _popup : gameObject;
        }

        private void CacheTextReferences()
        {
            if (_popup == null)
            {
                return;
            }

            if (_acceptButtonText == null && _acceptButton != null)
            {
                _acceptButtonText = _acceptButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (_rejectButtonText == null && _rejectButton != null)
            {
                _rejectButtonText = _rejectButton.GetComponentInChildren<TMP_Text>(true);
            }

            if (_messageText == null)
            {
                TMP_Text[] texts = _popup.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text text in texts)
                {
                    if (_acceptButton != null && text.transform.IsChildOf(_acceptButton.transform)) continue;
                    if (_rejectButton != null && text.transform.IsChildOf(_rejectButton.transform)) continue;
                    _messageText = text;
                    break;
                }
            }
        }
    }
}