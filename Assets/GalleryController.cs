using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gallerycontroller : MonoBehaviour
{
    [SerializeField]
    private List <GameObject> _characterList;
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject character in _characterList)
        {
            Transform content = transform.Find("Content");
            Instantiate(character,content);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
