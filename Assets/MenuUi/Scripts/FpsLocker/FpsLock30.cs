using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLock30 : MonoBehaviour
{
    public void ChangeFrameRateToNative()
    {
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

        PlayerPrefs.SetInt("TargetFrameRate", Application.targetFrameRate);
        PlayerPrefs.SetInt("TargetFrameRateNative", 1);
        PlayerPrefs.Save();
    }

    public void ChangeFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;

        PlayerPrefs.SetInt("TargetFrameRate", frameRate);
        PlayerPrefs.SetInt("TargetFrameRateNative", 0);
        PlayerPrefs.Save();
    }
}
