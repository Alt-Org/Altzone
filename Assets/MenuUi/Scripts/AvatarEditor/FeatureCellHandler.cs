using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeatureCellHandler : GridCellHandler
    {
        public void SetValues(Sprite cellImage,
            Color highlightColor,
            Color backgroundColor)
        {
            base.SetValues(cellImage,
                highlightColor,
                backgroundColor);
        }

        public new void Highlight(bool isHighlighted)
        {
            base.Highlight(isHighlighted);
        }

        public void SetOnClick(UnityEngine.Events.UnityAction onClick)
        {
            base.SetValues(onClick: onClick);
        }
    }
}
