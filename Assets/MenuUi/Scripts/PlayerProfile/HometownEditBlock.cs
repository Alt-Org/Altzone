using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HometownEditBlock : MonoBehaviour
{
    public GameObject popUp;
    public void onClick()
    {
        popUp.SetActive(true);
       // Wait();
       // popUp.SetActive(false);
    }

   // IEnumerator Wait()
  //  {
   //     yield return new WaitForSeconds(5);
 //   }
}
