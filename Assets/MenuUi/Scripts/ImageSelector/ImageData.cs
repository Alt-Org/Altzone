using System;
using UnityEngine;

namespace MenuUI.Scripts
{
    /// <summary>
    /// Used with ImageSelector. Holder class for image's show condition and sprite.
    /// </summary>
    [Serializable]
    public class ImageData
    {
        /// <summary>
        /// The condition why this image is shown.
        /// </summary>
        [SerializeField] public ImageSelector.ImageShowConditions ShowCondition;

        [SerializeField] public Sprite ImageSprite;
    }
}
