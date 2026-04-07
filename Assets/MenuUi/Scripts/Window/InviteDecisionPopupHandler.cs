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
                .FirstOrDefault(handler => handler != null
                    && handler.gameObject != null
                    && handler.gameObject.scene.IsValid());
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
                GameObject popupInstance = parent != null
                    ? Instantiate(prefab, parent, false)
                    : Instantiate(prefab);

                popupInstance.name = "InviteDecisionPanel";
                if (parent == null)
                {
                    DontDestroyOnLoad(popupInstance);
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
            GameObject overlayPanel = GameObject.FindWithTag("OverlayPanel");
            if (overlayPanel != null)
            {
                return overlayPanel.transform;
            }

            OverlayPanelCheck overlayPanelCheck = Resources.FindObjectsOfTypeAll<OverlayPanelCheck>()
                .FirstOrDefault(check => check != null && check.gameObject != null);
            if (overlayPanelCheck != null)
            {
                return overlayPanelCheck.transform;
            }

            if (InLobbyController.PopupContentsInstance != null)
            {
                return InLobbyController.PopupContentsInstance.transform;
            }

            Canvas fallbackCanvas = FindObjectOfType<Canvas>();
            return fallbackCanvas != null ? fallbackCanvas.transform : null;
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
            GetPopupTarget().SetActive(true);
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