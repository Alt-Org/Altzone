using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts
{
    /// <summary>
    /// Script for selecting a specific image depending on a condition.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ImageSelector : MonoBehaviour
    {
        /// <summary>
        /// Conditions for showing a image.
        /// </summary>
        public enum ImageShowConditions
        {
            IsOnLeaderboard,
            NotOnLeaderboard,
            VoteAvailable,
            VoteUnavailable,
        }

        [SerializeField] private List<ImageData> _images;
        private Image _image = null;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        /// <summary>
        /// Chooses and assigns a sprite to the Image component depending on the condition.
        /// </summary>
        /// <param name="condition">The condition which determines which image is shown.</param>
        public void SelectImage(ImageShowConditions condition)
        {
            if (_image != null && _images != null)
            {
                foreach (ImageData image in _images)
                {
                    if (image.ShowCondition == condition && image.ImageSprite != null)
                    {
                        _image.sprite = image.ImageSprite;
                        return;
                    }
                }
            }
        }
    }
}
