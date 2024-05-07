using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatsPieChart : MonoBehaviour
{

    // References for Unity
    public Image[] imagesPieChart;


    // Goes through every image representing a stat and gives it a fill amount creating a pie chart
    public void SetPieChartValues(float[] pieChartValuesToSet)
    {
        float totalValues = 0;
        for(int i = 0; i < imagesPieChart.Length; i++)
        {
            totalValues += FindPieChartPercentage(pieChartValuesToSet, i);
            imagesPieChart[i].fillAmount = totalValues;
        }
    }

    private float FindPieChartPercentage(float[] pieChartValuesToSet, int index)
    {
        float totalAmount = 0;
        for(int i = 0; i < pieChartValuesToSet.Length; i++)
        {
            totalAmount += pieChartValuesToSet[i];
        }

        return pieChartValuesToSet[index] / totalAmount;
    }
}
