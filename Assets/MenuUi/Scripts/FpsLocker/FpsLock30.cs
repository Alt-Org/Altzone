using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLock30 : MonoBehaviour {
[SerializeField] private int frameRate30;
[SerializeField] private int frameRate60;
    public void Framerate30() {
        Application.targetFrameRate = frameRate30;
    }
    public void Framerate60() {
        Application.targetFrameRate = frameRate60;
    }
}
