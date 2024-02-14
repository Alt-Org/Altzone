using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Button2Controller : MonoBehaviour
{
    // Reference to Button1
    public Button button1;

    void Start()
    {
        // Subscribe to Button2's click event
        GetComponent<Button>().onClick.AddListener(OnButton2Click);
    }

    // Method to be called when Button2 is clicked
    void OnButton2Click()
    {
        // Enable Button1
        button1.GetComponent<Button1Controller>().EnableButton1();
    }
}