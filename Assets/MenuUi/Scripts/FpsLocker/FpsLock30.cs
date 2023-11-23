using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLock30 : MonoBehaviour {

    public void ChangeFrameRateToNative()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }

    public void ChangeFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;
    }
}
