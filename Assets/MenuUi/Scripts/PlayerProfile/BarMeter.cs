using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Quantum;

public class BarMeter : MonoBehaviour
{
    private Slider slider;

    float carbonBarDisplay;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }


    private void Start()
    {
        slider.value = 0;
    }

    void Update()
    {
        carbonBarDisplay = CarbonFootprint.CarbonCount;
        UpdateValue();
    }

    public void UpdateValue()
    {
        slider.value = carbonBarDisplay;
    }
}
