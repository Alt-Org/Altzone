using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor{
    public class AvatarFaceLoader : MonoBehaviour
    {
        public AvatarVisualDataScriptableObject avatarVisuals;
        private List<Image> _faceImages;

        void Awake()
        {
            _faceImages = GetComponentsInChildren<Image>().ToList();
            // foreach(Image image in GetComponentsInChildren<Image>())
            // {
            //     _faceImages.Add(image);
            // }
        }
        void OnEnable()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // added null checks to avoid errors while testing
            if (avatarVisuals.sprites != null) return;
            if (avatarVisuals.colors != null) return;

            for (int i = 0; i < _faceImages.Count; i++)
            {
                _faceImages[i].sprite = avatarVisuals.sprites[i];
                _faceImages[i].color = avatarVisuals.colors[i];
            }
        }
    }
}
