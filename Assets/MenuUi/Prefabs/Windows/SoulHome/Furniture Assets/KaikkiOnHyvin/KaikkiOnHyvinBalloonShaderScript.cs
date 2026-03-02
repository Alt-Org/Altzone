using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinBalloonShaderScript : MonoBehaviour, ISoulHomeObjectClick
{
    public ParticleSystem confettiParticles;
    private SpriteRenderer renderer;
    private float strenght;
    private float noiseThickness;
    private float transitionDuration;
    private float currentAlpha;

    public GameObject balloon;

    void Start()
    {
        renderer = balloon.GetComponent<SpriteRenderer>();
        confettiParticles.Stop();
        resetValues();
    }

    public void HandleClick()
    {
        confettiParticles.Play();
        StartCoroutine(balloonPop());
    }

    private IEnumerator balloonPop()
    {     
        float elapsedTime = 0f;   
        transitionDuration = 0.2F;        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            strenght = Mathf.Lerp(0f, 1.5f, elapsedTime / transitionDuration);
            renderer.material.SetFloat("_Strenght", strenght);

            noiseThickness = Mathf.Lerp(2f, 0f, elapsedTime / transitionDuration);
            renderer.material.SetFloat("_NoiseThickness", noiseThickness);

            yield return null;
        }
        balloon.SetActive(false);
        yield return new WaitForSeconds(2);
        StartCoroutine(balloonFadeIn());
    }

    private IEnumerator balloonFadeIn()
    {
        resetValues();
        float elapsedTime = 0f;
        transitionDuration = 1F;       
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            currentAlpha = Mathf.Lerp(0f, 1.0f, elapsedTime / transitionDuration);
            renderer.color = new Color (1f, 1f, 1f, currentAlpha);

            yield return null;
        }

    }

    public void resetValues()
    {
        renderer.material.SetFloat("_Strenght", 0f);
        renderer.material.SetFloat("_NoiseThickness", 1.5f);
        balloon.SetActive(true);
    }
}
}