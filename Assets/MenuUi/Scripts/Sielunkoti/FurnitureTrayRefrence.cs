using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureTrayRefrence : MonoBehaviour
{
    [SerializeField]
    private GameObject standard;
    [SerializeField]
    private GameObject shortWide;

    public GameObject GetFurniture(string name)
    {
        if (name == "Standard") return standard;
        else if (name == "ShortWide") return shortWide;
        else return null;
    }
}
