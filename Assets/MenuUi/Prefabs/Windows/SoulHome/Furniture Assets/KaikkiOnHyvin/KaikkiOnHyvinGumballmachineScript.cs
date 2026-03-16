using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinGumballmachineScript : MonoBehaviour, ISoulHomeObjectClick
{   
    [SerializeField]
    private ParticleSystem partSys;
    
    private float transitionDuration = 4.0F;
    private float newScaleY;
    private float newScaleX;
    private float addition;
    private bool active = false;

    public void HandleClick()
    {    
        if (active)
        {return;}

        StartCoroutine(machineAnimation());
    }

    private IEnumerator machineAnimation()
    {
        active = true;
        float elapsedTime = 0f;
        var partSysVOL = partSys.velocityOverLifetime;

        float scaleX = this.transform.localScale.x;
        float scaleY = this.transform.localScale.y;
        float scaleZ = this.transform.localScale.z;

        float A = 3.0f;
        float x = 0f;
        float w = 4.0f;
        float p = 1.3f;
        float b = 4.0f;

        bool popupActive = false;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;

            newScaleY = scaleY + (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            newScaleX = scaleX - (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            transform.localScale = new Vector3(newScaleX, newScaleY, scaleZ);

            addition = 100 * Mathf.Exp(-2 * x) * Mathf.Cos(w * x + 4.3f);

            if ( !popupActive && elapsedTime > 2)
            {   
                popupActive = true;
                KaikkiOnHyvinPopupScript.Instance.popupController(); //Uses the script's instance to call popupController funktion
            }
            // Affects how fast and which direction particles move
            partSysVOL.orbitalZ = new ParticleSystem.MinMaxCurve(1.0f + addition, 4.0f + addition);

            yield return null;
        }  
        active = false;
    }
}
}