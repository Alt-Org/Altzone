using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.CanvasUtil
{
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerAutoMatch : MonoBehaviour
    {
        private const float DefaultLandscapeMatch = 0f;
        private const float DefaultPortraitMatch = 1f;

        [SerializeField] private float _landscapeMatch = DefaultLandscapeMatch;
        [SerializeField] private float _portraitMatch = DefaultPortraitMatch;
        [SerializeField] private float _pollingInterval = 1.0f;

        private CanvasScaler _canvasScaler;
        private int _width;
        private int _height;

        private void OnEnable()
        {
            var canvas = GetComponent<Canvas>();
            _canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (_canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                enabled = false;
                return;
            }
            StartCoroutine(ScreenResolutionPoller());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator ScreenResolutionPoller()
        {
            YieldInstruction delay = _pollingInterval > 0 ? new WaitForSeconds(_pollingInterval) : new WaitForFixedUpdate();
            for (; enabled;)
            {
                yield return delay;
                if (_height == Screen.height && _width == Screen.width)
                {
                    continue;
                }
                _width = Screen.width;
                _height = Screen.height;
                var match = _width > _height ? _landscapeMatch : _portraitMatch;
                Debug.Log($"matchWidthOrHeight w {_width} h {_height} : {_canvasScaler.matchWidthOrHeight} <- {match}");
                if (Mathf.Approximately(_canvasScaler.matchWidthOrHeight, match))
                {
                    continue;
                }
                _canvasScaler.matchWidthOrHeight = match;
            }
        }
    }
}