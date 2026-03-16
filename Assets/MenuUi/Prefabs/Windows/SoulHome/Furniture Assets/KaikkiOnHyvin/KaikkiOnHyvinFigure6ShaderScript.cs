using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinFigure6ShaderScript : MonoBehaviour, ISoulHomeObjectClick
{
    private float distortionSpeed;
    private float cellDensity;
    [SerializeField]  
    private SpriteRenderer figureRenderer;
    private float transitionDuration = 5.0f;

    [SerializeField] 
    private GameObject figure;
    private GameObject box;

    private bool boxOpened = false;
    [SerializeField] 
    private KaikkiOnHyvinBoxScript kaikkiOnHyvinBoxScript;
    
    public void HandleClick()
    {
        if (boxOpened)
        {return;}
        box = figure.transform.GetChild(0).gameObject;
        kaikkiOnHyvinBoxScript.boxHandler(box);
        boxOpened = true;
    }

    void Start()
    {
        figureRenderer = GetComponent<SpriteRenderer>();
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

            figureRenderer.material.SetFloat("_DistortionSpeed", distortionSpeed);

            cellDensity = 90.0f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);

            figureRenderer.material.SetFloat("_CellDensity", cellDensity);

            if( transitionDuration <= elapsedTime )
            {
                elapsedTime = 0f;
                x = 0f;
            }
            yield return null;
        }
    }    
}
}