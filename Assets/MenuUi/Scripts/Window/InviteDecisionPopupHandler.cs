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
        private static bool _isRoot;

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

                GameObject popupInstance = Instantiate(prefab);

                popupInstance.name = "InviteDecisionPanel";

                EnsureStandaloneCanvas(popupInstance);
                popupInstance.transform.SetAsLastSibling();

                InviteDecisionPopupHandler handler = popupInstance.GetComponent<InviteDecisionPopupHandler>();

                _instance = handler;
                return handler;
            }
            finally
            {
                _isInstantiating = false;
            }
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

            if (transform.parent == null) _isRoot = true;
            else _isRoot = false;
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
            else
            {
                Debug.LogWarning("Invite Popup was called but is not visible in hierarcy.");
            }

            transform.SetAsLastSibling();
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
            if (_isRoot) Destroy(gameObject);
        }

        private GameObject GetPopupTarget()
        {
            return _popup != null ? _popup : gameObject;
        }
    }
}
