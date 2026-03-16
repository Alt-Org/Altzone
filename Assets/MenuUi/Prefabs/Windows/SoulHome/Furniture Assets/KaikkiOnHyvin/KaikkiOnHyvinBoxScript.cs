using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaikkiOnHyvinBoxScript : MonoBehaviour
{
    private float newScaleY;
    private float newScaleX;

    private SpriteRenderer boxRenderer;
    [SerializeField] 
    private ParticleSystem confettiParticles;
    private GameObject box;

    public void boxHandler(GameObject pBox) //pBox is a parameter holding a value from a figure script
    {
        box = pBox;
        boxRenderer = box.GetComponent<SpriteRenderer>();
        StartCoroutine(boxAnimation());
        confettiParticles.Play();
        StartCoroutine(boxFade());
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

        float scaleX = box.transform.localScale.x;
        float scaleY = box.transform.localScale.y;
        float scaleZ = box.transform.localScale.z;

        float A = 3.5f;
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
            box.transform.localScale = new Vector3(newScaleX, newScaleY, scaleZ);

            yield return null;
        }  
    }

    void Start()
    {
        confettiParticles.Stop();   
    }
}
