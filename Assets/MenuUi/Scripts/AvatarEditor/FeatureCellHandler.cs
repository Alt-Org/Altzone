using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeatureCellHandler : GridCellHandler
    {
        private bool _isColorable = false;
        public bool IsColorable {  get { return _isColorable; } }
        public void SetValues(Sprite cellImage,
            bool isColorable)
        {
            base.SetValues(cellImage);
            _isColorable = isColorable;
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
