using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammus : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        // Spins the bullet.
        transform.Rotate(-1.5f, 0f, 0f, Space.Self);
    }
}
