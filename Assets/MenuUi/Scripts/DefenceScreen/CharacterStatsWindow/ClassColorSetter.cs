using Altzone.Scripts.ReferenceSheets;
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
        [SerializeField] private ClassColorReference _referenceSheet;
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
                _image.color = _referenceSheet.GetColor(_controller.GetCurrentCharacterClass());
            }
        }
    }
}
