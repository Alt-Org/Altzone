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

        public static OverlayPanelCheck Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            if (_overlayObject == null) _overlayObject = transform.Find("UIOverlayPanel").GetComponent<GameObject>();
        }

        private void OnEnable()
        {
            if (GameObject.FindWithTag("OverlayPanel") ? true : SceneManager.GetActiveScene().name != _allowedScene.SceneName) //If OverlayPanel can be found, return, otherwise check if this panel is allowed to be set active.
            {
                return;
            }
            else _overlayObject.SetActive(true);


            UpdateButtonContent();
        }

        public void UpdateButtonContent()
        {
            if (buttons == null || buttons.Length == 0) return;
            Debug.LogWarning(WindowManager.Get().FindIndex(WindowManager.Get().CurrentWindow));
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                WindowDef target = button.GetComponent<NaviButton>().NaviTarget;
                var windowManager = WindowManager.Get();
                var isCurrentWindow = windowManager.FindIndex(target) == 0;


                if (isCurrentWindow)
                {
                    button.transform.localScale = Vector3.one * 1.2f;
                    button.interactable = false;
                }
                else
                {
                    button.transform.localScale = Vector3.one;
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
        }

    }
}
