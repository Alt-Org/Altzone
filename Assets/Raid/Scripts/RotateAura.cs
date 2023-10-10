using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAura : MonoBehaviour
{
    [SerializeField] float Speed;
    Transform Rt;
    // Start is called before the first frame update
    void Start()
    {
        Rt = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Rt.Rotate(0.0f, 0.0f, 90.0f * Time.deltaTime * Speed);
    }
}