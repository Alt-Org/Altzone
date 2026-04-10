using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class OverlayPanelCheck : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayObject;
        [SerializeField] private SceneDef _allowedScene;

        [SerializeField] private Button[] buttons;

        [SerializeField] private GameObject _bottomBar;
        [SerializeField] private GameObject _chatBox;
        [SerializeField] private GameObject _buttonsBar;

        [SerializeField] private Button _onlineToggleButton;

        private bool _chatActive = true;

        public static OverlayPanelCheck Instance { get; private set; }
        public bool ChatActive => _chatActive;

        public delegate void ChatBarToggled(bool active);
        public static event ChatBarToggled OnChatBarToggled;

        public delegate void ToggleOnlinePlayerList(bool? active = null);
        public static event ToggleOnlinePlayerList OnToggleOnlinePlayerList;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                if(gameObject.tag is "OverlayPanel") Instance = this;
                else Destroy(gameObject);
                UpdateButtonContent();
            }

            if (_overlayObject == null) _overlayObject = transform.Find("UIOverlayPanel").GetComponent<GameObject>();
            _chatActive = true;
            buttons[2].transform.localScale = Vector3.one * 1.2f;
            //buttons[2].interactable = false;

        }

        private void OnEnable()
        {
            GameObject panel= GameObject.FindWithTag("OverlayPanel");
            if (panel != gameObject && panel ? true : SceneManager.GetActiveScene().name != _allowedScene.SceneName) //If OverlayPanel can be found, return, otherwise check if this panel is allowed to be set active.
            {
                return;
            }
            else _overlayObject.SetActive(true);

            if (Instance == this)
                UpdateButtonContent();

            _onlineToggleButton.onClick.AddListener(ToggleOnlinePlayers);
        }

        private void OnDisable()
        {
            _onlineToggleButton?.onClick.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            if(Instance == this) Instance = null;
        }

        public void UpdateButtonContent()
        {
            if (buttons == null || buttons.Length == 0) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                WindowDef target = button.GetComponent<NaviButton>().NaviTarget;
                var windowManager = WindowManager.Get();
                var isCurrentWindow = windowManager.FindIndex(target) == 0;
                var buttonEffect = button.GetComponent<LowerPanelEffect>();


                if (isCurrentWindow)
                {
                    button.transform.localScale = Vector3.one * 1.2f;
                    buttonEffect.activateEffect(true, button);
                    //button.interactable = false;
                }
                else
                {
                    button.transform.localScale = Vector3.one;
                    buttonEffect.activateEffect(false, button);
                    button.interactable = true;
                }
            }
        }

        public void ToggleBottomBar(bool value)
        {
            _bottomBar.SetActive(value);
        }

        public void ToggleChat(bool value)
        {
            _chatBox.SetActive(value);
            _chatActive = value;
            _buttonsBar.GetComponent<RectTransform>().anchorMax = value ? new Vector2(1, 0.5f) : new Vector2(1, 1f);
            OnChatBarToggled?.Invoke(value);
        }

        public void ToggleOnlinePlayers()
        {
            OnToggleOnlinePlayerList?.Invoke();
        }

    }
}
