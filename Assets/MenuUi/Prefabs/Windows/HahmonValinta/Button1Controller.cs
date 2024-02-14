using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Button1Controller : MonoBehaviour
{
    // Reference to Button2
    public Button button2;

    void Start()
    {
        // Initially disable Button1
        GetComponent<Button>().interactable = false;
    }

    // Method to be called when Button2 is clicked
    public void EnableButton1()
    {
        GetComponent<Button>().interactable = true;
    }
}
