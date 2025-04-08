using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Sets class color to a image component.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ClassColorSetter : MonoBehaviour
    {
        private Image _image;
        private StatsWindowController _controller;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _controller = FindObjectOfType<StatsWindowController>();
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                Color classColor = _controller.GetCurrentCharacterClassAlternativeColor();
                _image.color = new Color(classColor.r, classColor.g, classColor.b, _image.color.a);
            }
        }
    }
}
