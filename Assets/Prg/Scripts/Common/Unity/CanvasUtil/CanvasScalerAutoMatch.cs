using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.CanvasUtil
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasScalerAutoMatch : MonoBehaviour
    {
        private const float DefaultLandscapeMatch = 0f;
        private const float DefaultPortraitMatch = 1f;

        [SerializeField] private float _landscapeMatch = DefaultLandscapeMatch;
        [SerializeField] private float _portraitMatch = DefaultPortraitMatch;

        private CanvasScaler _canvasScaler;
        private int width;
        private int height;

        private void Start()
        {
            var canvas = GetComponent<Canvas>();
            _canvasScaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(_canvasScaler, "_canvasScaler != null");
        }

        private void Update()
        {
            if (height == Screen.height && width == Screen.width)
            {
                return;
            }
            if (_canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return;
            }
            height = Screen.height;
            width = Screen.width;
            var match = width < height ? _portraitMatch : _landscapeMatch;
            if (!Mathf.Approximately(_canvasScaler.matchWidthOrHeight, match))
            {
                _canvasScaler.matchWidthOrHeight = match;
            }
        }
    }
}