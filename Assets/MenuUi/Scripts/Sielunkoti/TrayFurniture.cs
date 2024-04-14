using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class TrayFurniture : MonoBehaviour
    {
        [SerializeField]
        private GameObject _furnitureObject;
        private Furniture _furniture;
        [SerializeField]
        private  int debugValue = 0;

        public GameObject FurnitureObject { get => _furnitureObject; set => _furnitureObject = value; }
        public Furniture Furniture { get => _furniture; set => _furniture = value; }


        // Start is called before the first frame update
        void Start()
        {
            if (debugValue == 1)
                Furniture = new Furniture(1, "Standard", new Vector2Int(-1, -1), FurnitureSize.OneXTwo, 15f);
            else if(debugValue == 2)
                Furniture = new Furniture(2, "ShortWide", new Vector2Int(-1, -1), FurnitureSize.OneXFour, 15f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
