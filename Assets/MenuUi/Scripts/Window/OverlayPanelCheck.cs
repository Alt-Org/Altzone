using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUi.Scripts.Window
{
    public class OverlayPanelCheck : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayObject;
        [SerializeField] private SceneDef _allowedScene;

        private void Awake()
        {
            if (_overlayObject == null) _overlayObject = transform.Find("UIOverlayPanel").GetComponent<GameObject>();
        }

        private void OnEnable()
        {
            if (GameObject.FindWithTag("OverlayPanel") ? true : SceneManager.GetActiveScene().name != _allowedScene.SceneName) //If OverlayPanel can be found, return, otherwise check if this panel is allowed to be set active.
            {
                return;
            }
            else _overlayObject.SetActive(true);

        }
    }
}
