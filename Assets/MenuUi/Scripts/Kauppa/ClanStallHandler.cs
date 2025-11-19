using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClanStallPopupHandler : MonoBehaviour
{

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
    
    public void OpenPopup()
    {
        gameObject.SetActive(true);
    }

}
