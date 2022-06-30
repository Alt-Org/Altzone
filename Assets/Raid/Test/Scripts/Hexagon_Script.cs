using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon_Script : MonoBehaviour
{
    public float scanDistance;
    public Object neighborScanPosition;
    public Object neighborScanPosition2;
    public Object neighborScanPosition3;
    public Object neighborScanPosition4;
    public Object neighborScanPosition5;
    public Object neighborScanPosition6;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScanNeighbors()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, scanDistance))
        {
            Debug.Log("hit");
        }
        else
            Debug.Log("hit nothing");
    }
}
