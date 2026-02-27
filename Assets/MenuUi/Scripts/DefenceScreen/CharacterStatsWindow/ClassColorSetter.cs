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

        [SerializeField]private bool _setAlternativeColor = true;


        private void OnEnable()
        {
            
            if(_image==null)_image= GetComponent<Image>();
            if(_controller==null)_controller = FindObjectOfType<StatsWindowController>();

            if (_controller != null)
            {
                Color classColor = _setAlternativeColor
                    ?_controller.GetCurrentCharacterClassAlternativeColor()
                :_controller.GetCurrentCharacterClassColor();


                _image.color = new Color(classColor.r, classColor.g, classColor.b, _image.color.a);
            }
        }
    }
}
