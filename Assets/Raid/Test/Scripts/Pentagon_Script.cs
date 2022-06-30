using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pentagon_Script : MonoBehaviour
{
    public GameObject tagHit;
    public float rayDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var firstRay = new Ray(this.transform.position, this.transform.up);
        var secondRay = new Ray(this.transform.position, this.transform.forward);

        RaycastHit hit;

        if(Physics.Raycast(firstRay, out hit, rayDistance))
        {
            Debug.Log(hit.transform.tag);
            tagHit = (hit.transform.gameObject);
        }
        //Physics.Raycast(firstRay);
        //Physics.Raycast(secondRay);

    }
}
