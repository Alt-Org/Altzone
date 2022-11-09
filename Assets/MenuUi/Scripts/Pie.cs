using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pie : MonoBehaviour
{
   public Image[] imagesPieChart;
   public float[] values;
  

    void Start() {
         SetValues(values);  
           
    }

    
    void Update()
    {
     
    }

    public void SetValues(float[] valuesToSet)
    {
        float totalValues = 0;
        for(int i = 0; i < imagesPieChart.Length; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            imagesPieChart[i].fillAmount = totalValues;
        }
    }

    private float FindPercentage(float[] valueToSet, int index)
    {
        float totalAmount = 0;
        for(int i = 0; i < valueToSet.Length; i++)
        {
            totalAmount += valueToSet[i];
        }
        return valueToSet[index] / totalAmount;
    }
   
  
        
	
}
