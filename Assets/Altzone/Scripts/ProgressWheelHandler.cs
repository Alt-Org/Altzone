using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressWheelHandler : MonoBehaviour
{
    public static ProgressWheelHandler Instance; // singleton
    public GameObject Wheel;
    private void Awake()
    {
        Instance = this; // set singleton
    }
}
