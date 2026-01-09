using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class CategoryCellHandler : GridCellHandler
    {
        public void SetValues(Sprite cellImage, Color backgroundColor)
        {
            base.SetValues(cellImage: cellImage, backgroundColor: backgroundColor);
        }

        public void SetOnClick(UnityEngine.Events.UnityAction onClick)
        {
            base.SetValues(onClick: onClick);
        }

        public void ButtonIsInteractable(bool isInteractable)
        {
            base.SetValues(buttonIsInteractable: isInteractable);
        }
    }
}
