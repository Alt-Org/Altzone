using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorCellHandler : GridCellHandler
    {
        public void SetColor(Color color)
        {
           base._featureImage.color = color;
        }

        public void SetOnClick(UnityEngine.Events.UnityAction onClick)
        {
            base.SetValues(onClick: onClick);
        }
    }
}
