using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaikkiOnHyvinFihure6ShaderScript : MonoBehaviour
{
    private float distortionSpeed;
    private float cellDensity;
    public SpriteRenderer renderer;
    private float transitionDuration = 5.0f;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DistortionAnimation());
        
    }
    private IEnumerator DistortionAnimation()
    {
        float elapsedTime = 0f;
        float x = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;

            distortionSpeed = 50.0f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);    

            renderer.material.SetFloat("_DistortionSpeed", distortionSpeed);

            cellDensity = 90.0f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);

            renderer.material.SetFloat("_CellDensity", cellDensity);

            if( transitionDuration <= elapsedTime )
            {
                elapsedTime = 0f;
                x = 0f;
            }
            yield return null;
        }
    }    
}
