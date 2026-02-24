using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaikkiOnHyvinFigure2ShaderScript : MonoBehaviour
{
    private float distortionSpeed;
    private float distortionPosition;
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
