using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressBar : MonoBehaviour
{

    [SerializeField] private Slider levelProgressBar;

    private float currentProgressValue = 0f;

    private float maxValue = 100f;
    private float step = 10f;

    private void Awake() 
    {
        levelProgressBar.value = currentProgressValue;
    }

    private void IncreaseLevelProgress() //=> currentProgressValue = (currentProgressValue + step) % 101;
    {
        if (currentProgressValue >= 100)
        {
            currentProgressValue = 0;
        }
        currentProgressValue += step;
    }
}
