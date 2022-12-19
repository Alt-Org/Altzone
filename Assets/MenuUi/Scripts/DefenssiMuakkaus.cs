using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefenssiMuakkaus : MonoBehaviour
{
    public float[] values;
    public float incrementAmount;
    public Text valueText;

    public Button[] valueButtons;
    public Button incrementButton;
    public Button decrementButton;

    private int selectedIndex = 0;

    void Start()
    {
        for (int i = 0; i < valueButtons.Length; i++)
        {
            int index = i;
            valueButtons[i].onClick.AddListener(() => SelectValue(index));
        }

        incrementButton.onClick.AddListener(IncrementValue);
        decrementButton.onClick.AddListener(DecrementValue);
    }

    void SelectValue(int index)
    {
        selectedIndex = index;
    }

    void IncrementValue()
    {
        values[selectedIndex] += incrementAmount;
        UpdateValueText();
    }

    void DecrementValue()
    {
        values[selectedIndex] -= incrementAmount;
        UpdateValueText();
    }

    void UpdateValueText()
    {
        valueText.text = 
            "<color=red>" + values[0] + "</color>, " +
            "<color=blue>" + values[1] + "</color>, " +
            "<color=lightblue>" + values[2] + "</color>, " +
            "<color=yellow>" + values[3] + "</color>";
    }
}