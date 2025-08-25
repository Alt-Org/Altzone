using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLock30 : MonoBehaviour {

    public void ChangeFrameRateToNative()
    {
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        PlayerPrefs.SetInt("TargetFrameRate", Application.targetFrameRate);
    }

    public void ChangeFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;
        PlayerPrefs.SetInt("TargetFrameRate", Application.targetFrameRate);
    }
}
