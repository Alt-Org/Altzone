using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HometownEditBlock : MonoBehaviour
{
    public GameObject popUp;
    bool switcher = false;
    public void onClick()
    {
        if (switcher == false)
        {
            popUp.SetActive(true);
            switcher = true;
        }
        else
        {
            popUp.SetActive(false);
            switcher = false;
        }
    }
}
