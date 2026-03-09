using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinFigure2ShaderScript : MonoBehaviour, ISoulHomeObjectClick
{
    private float distortionSpeed;
    private float distortionPosition;
    private float transitionDuration = 5.0f;
    private float newScaleY;
    private float newScaleX;

    public SpriteRenderer renderer;
    private SpriteRenderer boxRenderer;
    public ParticleSystem confettiParticles;

    public GameObject figure;
    private GameObject box;

    private bool boxOpened = false;

    public void HandleClick()
    {
        if (boxOpened)
        {return;}
        boxHandler();

    }

    public void boxHandler()
    {

        box = figure.transform.GetChild(0).gameObject;
        boxRenderer = box.GetComponent<SpriteRenderer>();
        StartCoroutine(boxAnimation());
        StartCoroutine(boxFade());
        confettiParticles.Play();
        boxOpened = true;
        Destroy(box, 1f);

    }

    private IEnumerator boxFade()
    {
        float elapsedTime = 0f;
        float currentAlpha = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;

            currentAlpha = Mathf.Lerp(1f, 0f, elapsedTime / 1f);
            boxRenderer.color = new Color (1f, 1f, 1f, currentAlpha);

            yield return null;
        }
    }
    
    private IEnumerator boxAnimation()
    {
        float elapsedTime = 0f;

        float scaleX = this.transform.localScale.x;
        float scaleY = this.transform.localScale.y;
        float scaleZ = this.transform.localScale.z;

        float A = 3.0f;
        float x = 0f;
        float w = 4.0f;
        float p = 1.3f;
        float b = 4.0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;
            
            newScaleY = scaleY + (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            newScaleX = scaleX - (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            transform.localScale = new Vector3(newScaleX, newScaleY, scaleZ);

            yield return null;
        }  
    }

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DistortionAnimation());
        confettiParticles.Stop();
        
    }
    private IEnumerator DistortionAnimation()
    {
        float elapsedTime = 0f;
        float x = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;

            distortionSpeed = 5.0f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);    
            //distortionSpeed = Mathf.Lerp(0f, 2f, elapsedTime / transitionDuration); // only hue changes
            renderer.material.SetFloat("_DistortionSpeed", distortionSpeed);

            distortionPosition = 1.1f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);
            //distortionPosition = Mathf.Lerp(0f, 0.25f, elapsedTime / transitionDuration);
            renderer.material.SetVector("_distortionPosition", new Vector2(0, distortionPosition));

            if( transitionDuration <= elapsedTime ) // to ensure continuty
            {
                elapsedTime = 0f;
                x = 0f;
            }
            yield return null;
        }
        
    }

}
}