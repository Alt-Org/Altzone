using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatsPieChart : MonoBehaviour
{
    // References for Unity
    public Image[] imagesPieChart;
    public float[] pieChartValues;

    // Start is called before the first frame update
    void Start()
    {
        SetPieChartValues(pieChartValues);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Goes through every image representing a stat and gives it a fill amount creating an illusion of a pie chart
    // We can use these functions when we get stats on the server
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
