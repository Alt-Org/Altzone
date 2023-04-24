using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scrollings : MonoBehaviour
{
    [Header("Settings"), SerializeField] private GameObject _CellPrefab;
    // Start is called before the first frame update

    void Start()
    {
        /* for (int i = 0; i <= 10; i++) 
         {
             GameObject obj = Instantiate(_CellPrefab);
             obj.transform.SetParent(gameObject.transform, false);
             obj.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
         }
        */
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    
}
