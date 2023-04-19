using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Scrollings : MonoBehaviour
{
    [SerializeField] private GameObject CellPrefab;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <= 10; i++) {
            GameObject obj = Instantiate(CellPrefab);
            obj.transform.SetParent(this.gameObject.transform,false);
            obj.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
        }
    }

}
