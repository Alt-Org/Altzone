using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinFigure2ShaderScript : MonoBehaviour, ISoulHomeObjectClick
{
    private float distortionSpeed;
    private float distortionPosition;
    private float transitionDuration = 5.0f;

    [SerializeField]  
    private SpriteRenderer figureRenderer;
    [SerializeField]  
    private GameObject imagePrefab;
    //[SerializeField]  
    //private Sprite imageBox;
    [SerializeField] 
    private GameObject figure;
    private GameObject box;
    private Image imageComp;
    [SerializeField] 
    private Sprite imageFigure;

    private bool boxOpened = false;
    [SerializeField] 
    private KaikkiOnHyvinBoxScript kaikkiOnHyvinBoxScript;

    public void HandleClick()
    {
        Debug.Log("FIRST LINE IN HANDLE CLICK, BOX OPENED:" + boxOpened);
        if (boxOpened)
        {return;}
        imageComp.sprite = imageFigure;
        box = figure.transform.GetChild(0).gameObject;
        kaikkiOnHyvinBoxScript.boxHandler(box);
        Debug.Log("AFTER BOXHANDLER, BOX OPENED:" + boxOpened);
        boxOpened = true;
    }

    void Start()
    {
        imageComp = imagePrefab.GetComponent<Image>();
        //imageComp.sprite = imageBox;
        
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

            distortionSpeed = 5.0f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);    
            figureRenderer.material.SetFloat("_DistortionSpeed", distortionSpeed);

            distortionPosition = 1.1f * Mathf.Exp(-1.0f * x) * Mathf.Cos(3.0f * x + 1.3f);
            figureRenderer.material.SetVector("_distortionPosition", new Vector2(0, distortionPosition));

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