using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.CanvasUtil
{
    /// <summary>
    /// Sets <c>CanvasScaler</c> matchWidthOrHeight value based on screen width and height in order to force portrait or landscape match value. 
    /// </summary>
    /// <remarks>
    /// Requires <c>ScaleMode.ScaleWithScreenSize</c> to work!<br />
    /// This is stupid way to poll for changes in game window width or height using screen resolution as measurement stick.
    /// </remarks>
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
            _canvasScaler = gameObject.GetComponent<CanvasScaler>();
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
            YieldInstruction delay = _pollingInterval > 0 ? new WaitForSeconds(_pollingInterval) : null;
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
                if (Mathf.Approximately(_canvasScaler.matchWidthOrHeight, match))
                {
                    continue;
                }
                Debug.Log($"matchWidthOrHeight w {_width} h {_height} : {_canvasScaler.matchWidthOrHeight} <- {match}",
                    _canvasScaler.gameObject);
                _canvasScaler.matchWidthOrHeight = match;
            }
        }
    }
}