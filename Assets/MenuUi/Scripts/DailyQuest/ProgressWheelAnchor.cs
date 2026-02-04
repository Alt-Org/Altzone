using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressWheelAnchor : MonoBehaviour
{
    private void OnEnable()
    {
        ProgressWheelHandler.OnSetWheelPosition += FetchWheel;
    }

    private void OnDisable()
    {
        ProgressWheelHandler.OnSetWheelPosition -= FetchWheel;
    }

    private void FetchWheel(Transform transfrom)
    {
        ProgressWheelHandler.Instance.transform.SetParent(this.transform);
    }
}
