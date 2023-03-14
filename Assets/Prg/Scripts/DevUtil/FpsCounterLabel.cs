using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.DevUtil
{
    public class FpsCounterLabel : MonoBehaviour
    {
        [Header("This only runs in Editor or Development Build")] public bool _visible;
        public Key _controlKey = Key.F2;
        public Key _shiftKey = Key.LeftShift;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private readonly GUIStyle _guiLabelStyle = new();
        private readonly Rect _guiLabelPosition = new(5, 5, 100, 25);

        private float _timeLeft;
        private float _accumulator;
        private int _frames;
        private float _fps;

        private IEnumerator Start()
        {
            if (FindObjectsOfType(GetType()).Length > 1)
            {
                enabled = false;
                yield break;
            }
            _guiLabelStyle.fontStyle = FontStyle.Bold;
            _guiLabelStyle.normal.textColor = Color.white;
            var savedVisible = _visible;
            _visible = false;
            yield return new WaitForSeconds(1f);
            _visible = savedVisible;
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame && Keyboard.current[_shiftKey].isPressed)
            {
                ToggleWindowState();
            }
            _accumulator += Time.timeScale / Time.deltaTime;
            ++_frames;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0.0)
            {
                _fps = (_accumulator / _frames);
                _timeLeft = 0.5f;
                _accumulator = 0.0f;
                _frames = 0;
            }
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }
            var label = $"{_fps:0} fps";
            GUI.Label(_guiLabelPosition, label, _guiLabelStyle);
        }

        private void ToggleWindowState()
        {
            _visible = !_visible;
        }
#endif
    }
}