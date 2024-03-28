using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer_Controller : MonoBehaviour
{
    //Tämä on perus pause ja resume koodi. Eli en halua että kello liikkuu eteenpäin.
public  void Pause()
{
    Time.timeScale = 0f;
}
 
public void Resume()
{
     Time.timeScale = 1f;
}

}