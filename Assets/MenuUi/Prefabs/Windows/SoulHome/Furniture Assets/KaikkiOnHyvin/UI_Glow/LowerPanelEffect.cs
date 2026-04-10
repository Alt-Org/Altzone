using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowerPanelEffect : MonoBehaviour
{
    [SerializeField] private Image glowEffect;
    //private Button pButton;

    public void activateEffect(bool boolean, Button pButton )
    {
        glowEffect.enabled = boolean;
        Debug.Log("GLOW EFFECT: " + glowEffect + "BUTTON: " + pButton);
    }
}
