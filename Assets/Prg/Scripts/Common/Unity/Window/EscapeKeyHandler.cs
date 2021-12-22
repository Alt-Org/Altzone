using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Tracks Escape key press so that is must pressed down and released exactly once (on the same level)
    /// on behalf of <c>WindowManager</c>.
    /// </summary>
    public class EscapeKeyHandler : MonoBehaviour
    {
        private bool _isEscapePressedDown;
        private bool _isEscapePressedUp;
        private string _activeScenePathDown;
        private string _activeScenePathUp;

        private Action _callback;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                _activeScenePathDown = SceneManager.GetActiveScene().path;
                _isEscapePressedDown = true;
                _isEscapePressedUp = false;
                return;
            }
            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
            {
                _activeScenePathUp = SceneManager.GetActiveScene().path;
                _isEscapePressedUp = true;
                return;
            }
            if (_isEscapePressedDown && _isEscapePressedUp)
            {
                _isEscapePressedDown = false;
                _isEscapePressedUp = false;
                if (_activeScenePathDown != _activeScenePathUp)
                {
                    Debug.LogWarning($"ESCAPE SKIPPED down={_activeScenePathDown} up={_activeScenePathUp}");
                    return;
                }
                _callback?.Invoke();
            }
        }

        public void SetCallback(Action callback)
        {
            _callback = callback;
        }
    }
}