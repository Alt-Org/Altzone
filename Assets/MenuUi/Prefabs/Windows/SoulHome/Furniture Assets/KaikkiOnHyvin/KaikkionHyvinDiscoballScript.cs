using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkionHyvinDiscoballScript : MonoBehaviour, ISoulHomeObjectClick
{
    public GameObject lights;
    private SpriteRenderer renderer;
    private float transitionDuration = 10.0F;
    private float elapsedTime = 0F;
    private float currentHue;

    public void HandleClick()
    {
        Debug.Log("IN HANDLECLICK()");
        lightSwitch();
    }

    public void lightSwitch()
    {
        if (lights.activeSelf)
        {
            lights.SetActive(false);
        }
        else
        {
            lights.SetActive(true);
            StartCoroutine(lightHueChange());
            int randomChance = Random.Range(1,4); // 1 in 4 chance the lights flicker when lights are turned on
            if(randomChance == 3)
            {
                StartCoroutine(lightFlicker());
            }
        }
    }

    public IEnumerator lightHueChange()
    {
        while (elapsedTime < transitionDuration && lights.activeSelf)
        {
            elapsedTime += Time.deltaTime;

            currentHue = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration); // only hue changes
            
            renderer.color = Color.HSVToRGB(currentHue, 0.35f, 1f); // Unity uses RGB so HSV values must be changed to RGB values
            
            if( transitionDuration <= elapsedTime ) // to ensure continuty
            {
                elapsedTime = 0f;
                currentHue = 0f;
            }
            yield return null;
        }
        
    }


    public IEnumerator lightFlicker()
    {
        yield return new WaitForSeconds(1f);
        lights.SetActive(false);
        yield return new WaitForSeconds(0.7f);
        lights.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        lights.SetActive(false);
    }

    void Start()
    {
        renderer = lights.GetComponent<SpriteRenderer>();
        lights.SetActive(false);
    }

}
}