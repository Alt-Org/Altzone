using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlowTest : MonoBehaviour
{
    private GameObject glowEffect;
    public static GlowTest Instance; 

    public void activateEffect(Button pButton) //Button from OverlayPanelCheck.cs
    {
        try
        {
        glowEffect = pButton.transform.GetChild(0).gameObject; 
        Debug.Log("Sibling Index glow: " + glowEffect.transform.GetSiblingIndex());
        Debug.Log("Sibling Index button: " + pButton.transform.GetSiblingIndex());
        glowEffect.SetActive(true);
        }
        catch (System.Exception e)
        {
            
            Debug.Log("E.MESSAGE ACTIVATEEFFECT: " + e.Message + "BUTTON: " + pButton);

        }
    }

    public void disableEffect(Button pButton)
    {
        try
        {
        glowEffect = pButton.transform.GetChild(0).gameObject;
        Debug.Log("GETCHILD0 = " + glowEffect);
        glowEffect.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.Log("E.MESSAGE DISABLEEFFECT: " + e.Message + "BUTTON: " + pButton);
        }
    }

    void Awake()
    {
        Instance = this; 
    }
}
