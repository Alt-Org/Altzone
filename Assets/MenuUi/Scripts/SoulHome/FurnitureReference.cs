using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureReference : MonoBehaviour
    {
        [SerializeField]
        private GameObject standard;
        [SerializeField]
        private GameObject shortWide;
        [SerializeField]
        private List<GameObject> _furnitureList;

        public GameObject GetFurniture(string name)
        {
            if (name == "Standard") return standard;
            else if (name == "ShortWide") return shortWide;
            else
            {
                foreach (GameObject furniture in _furnitureList)
                {
                    if (furniture.GetComponent<FurnitureHandling>().Name == name) return furniture;
                }
            }
            return null;
        }
    }
}
