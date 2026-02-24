using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinBalloonShaderScript : MonoBehaviour, ISoulHomeObjectClick
{
    private SpriteRenderer renderer;
    private float strenght;
    private float noiseThickness;
    private float transitionDuration = 0.2F;

    public GameObject balloon;

    // Start is called before the first frame update
    void Start()
    {
        renderer = balloon.GetComponent<SpriteRenderer>();
        resetValues();
    }

    public void HandleClick()
    {
        StartCoroutine(balloonPop());
    }

    private IEnumerator balloonPop()
    {     
        float elapsedTime = 0f;   
        Debug.Log("balloonPop");            
        while (elapsedTime < transitionDuration)
        {
            

            elapsedTime += Time.deltaTime;

            strenght = Mathf.Lerp(0f, 1.5f, elapsedTime / transitionDuration);
            renderer.material.SetFloat("_Strenght", strenght);

            noiseThickness = Mathf.Lerp(2f, 0f, elapsedTime / transitionDuration);
            renderer.material.SetFloat("_NoiseThickness", noiseThickness);

            yield return null;
        }
        Debug.Log("balloonPop ENDING");
    }

    public void resetValues()
    {
        renderer.material.SetFloat("_Strenght", 0f);
        renderer.material.SetFloat("_NoiseThickness", 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}