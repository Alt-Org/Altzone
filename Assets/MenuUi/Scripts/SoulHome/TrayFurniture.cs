using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class TrayFurniture : MonoBehaviour
    {
        [SerializeField]
        private GameObject _furnitureObject;
        private Furniture _furniture;
        private FurnitureListObject _furnitureList;
        [SerializeField]
        private  int debugValue = 0;

        public GameObject FurnitureObject { get => _furnitureObject; set {if (!Application.isPlaying) _furnitureObject = value; } }
        public Furniture Furniture { get => _furniture; set => _furniture = value; }
        public FurnitureListObject FurnitureList { get => _furnitureList; set => _furnitureList = value; }


        // Start is called before the first frame update
        void Start()
        {
            ScaleSprite(GetComponent<Image>().sprite, GetComponent<RectTransform>());

            if (debugValue == 1)
                Furniture = new Furniture(1, "Standard", new Vector2Int(-1, -1), FurnitureSize.OneXTwo, FurnitureSize.OneXOne, FurniturePlacement.Floor, 10f, 15f, false);
            else if(debugValue == 2)
                Furniture = new Furniture(2, "ShortWide", new Vector2Int(-1, -1), FurnitureSize.OneXFour, FurnitureSize.OneXOne, FurniturePlacement.Floor, 10f, 15f, false);
        }

        private void ScaleSprite(Sprite sprite, RectTransform rTransform)
        {
            float parentHeight = transform.parent.GetComponent<RectTransform>().rect.height;
            float initialHeight = parentHeight * 0.7f;
            rTransform.sizeDelta = new(initialHeight, initialHeight);
            Rect imageRect = rTransform.rect;
            if (sprite.bounds.size.x > sprite.bounds.size.y)
            {
                float diff = sprite.bounds.size.x / sprite.bounds.size.y;
                float newHeight = imageRect.height / diff;
                rTransform.sizeDelta = new(imageRect.width, (newHeight));
            }
            if (sprite.bounds.size.x < sprite.bounds.size.y)
            {
                float diff = sprite.bounds.size.y / sprite.bounds.size.x;
                float newWidth = imageRect.width / diff;
                rTransform.sizeDelta = new((newWidth), imageRect.height);
            }
        }
    }
}
