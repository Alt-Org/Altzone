using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressWheelSecondsHandler : MonoBehaviour
{
    public static ProgressWheelSecondsHandler Instance;

    public GameObject WheelSeconds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
