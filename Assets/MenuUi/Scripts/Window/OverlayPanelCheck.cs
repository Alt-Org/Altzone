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

        private int _currentPage = 2;

        public static OverlayPanelCheck Instance { get; private set; }

        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (!gameObject.activeInHierarchy) return;
                if (_currentPage != value)
                {
                    _currentPage = value;
                    UpdateButtonContent();
                }
            }
        }

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

        private void UpdateButtonContent()
        {
            if (buttons == null || buttons.Length == 0) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == CurrentPage)
                {
                    Button button = buttons[i];
                    button.transform.localScale = Vector3.one * 1.2f;
                    button.interactable = false;
                }
                else
                {
                    Button button = buttons[i];
                    button.transform.localScale = Vector3.one;
                    button.interactable = true;
                }
            }
        }

    }
}
