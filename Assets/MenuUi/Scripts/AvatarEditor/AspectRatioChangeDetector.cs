using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatioChangeDetector : MonoBehaviour
{
    public static event Action OnAspectRatioChange;
    private float _lastAspectRatio;
    private float _threshold = 0.01f;

    private void Start()
    {
        _lastAspectRatio = (float)Screen.width / Screen.height;
    }

    private void Update()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;

        if (Mathf.Abs(currentAspectRatio - _lastAspectRatio) > _threshold)
        {
            _lastAspectRatio = currentAspectRatio;
            StartCoroutine(InvokeOnNextFrame());
        }
    }

    private IEnumerator InvokeOnNextFrame()
    {
        yield return new WaitForEndOfFrame();
        OnAspectRatioChange.Invoke();
    }
}
