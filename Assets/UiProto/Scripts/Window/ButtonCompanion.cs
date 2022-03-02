using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UiProto.Scripts.Window
{
    [RequireComponent(typeof(Button))]
    public class ButtonCompanion : MonoBehaviour, IPointerEnterHandler
    {
        public Button button;
        public Color highlightedColor;

        private void Start()
        {
            button = GetComponent<Button>();
            var colors = button.colors;
            highlightedColor = colors.highlightedColor;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (button.transition == Selectable.Transition.ColorTint)
            {
                var colors = button.colors;
                if (WindowManager._Instance.useHighlightedColor)
                {
                    colors.highlightedColor = WindowManager._Instance.highlightedColor;
                }
                else
                {
                    colors.highlightedColor = highlightedColor;
                }
                button.colors = colors;
            }
        }
    }
}