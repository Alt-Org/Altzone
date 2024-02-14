using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HahmonValinta : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

        // Start is called before the first frame update
    void Start()
    {
        // Assign the onClick event for each button
        for (int i = 0; i < buttons.Length; i++)
        {
            int buttonIndex = i; // Capture the current value of i
            buttons[i].onClick.AddListener(() => ButtonClicked(buttonIndex));

            //Make all buttons green wwhen the game starts
            Image buttonImage = buttons[i].GetComponent<Image>();
            buttonImage.color = Color.green;
        }
    }

    // Method to handle button click
    void ButtonClicked(int clickedButtonIndex)
    {
        // Loop through each button in the array
        for (int i = 0; i < buttons.Length; i++)
        {
            // Get the Image component of the button
            Image buttonImage = buttons[i].GetComponent<Image>();

            // Check if this is the button that was clicked
            if (i == clickedButtonIndex)
            {
                buttonImage.color = Color.red;
            }
            else
            {
                buttonImage.color = Color.green;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSelectButton()
    {
        GetComponent<Button>().interactable = false;
    }
}
