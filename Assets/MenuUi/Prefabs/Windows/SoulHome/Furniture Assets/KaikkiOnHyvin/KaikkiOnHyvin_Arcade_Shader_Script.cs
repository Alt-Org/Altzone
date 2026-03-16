using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class NewBehaviourScript : MonoBehaviour, ISoulHomeObjectClick
{
    int round = 1;
    //Material shaderMat = GetComponent<Renderer>().material; 
    public SpriteRenderer screenRenderer;
    //private SpriteRenderer frontScreenRenderer;
    //private SpriteRenderer sideScreenRenderer;
    public GameObject arcadeMachine;
    public GameObject arcadeScreen;
    public GameObject frontScreen;
    public GameObject sideScreen;

    private float voronoiSpeedA;
    private float voronoiSpeedB;
    private float gradientNoiseSpeedA;
    private float gradientNoiseSpeedB;
    private Color screenColorA;
    private Color screenColorB;
    public SpriteRenderer currentSpriteRenderer;
    private Sprite currentSprite;

    private float transitionDuration = 1.0F;
    bool transitionInProgress = false;

    Color[] colors = {
        new Color(0.996f, 0.624f, 0.765f, 1f),new Color(0.627f, 1f, 1f, 1f),
        new Color(0.654f, 1f, 0.623f, 1f), new Color(1f, 0.921f, 0.616f, 1f),
        new Color(0.874f, 0.65f, 1f, 1f), new Color(1f, 0.525f, 0.352f, 1f)
        };
    float[] voronoiSpeeds ={ 0.5F,1.0F,2.5F,5.5F,9.5F};
    float[] gradientNoiseSpeeds ={ 0.1F,0.2F,0.3F,0.9F,1.5F};

    public void HandleClick() //comes from ISoulHomeObjectClick
    {   
        //spriteHandler();  
        Debug.Log("CURRENT SPRITE" + currentSprite);
        if (!transitionInProgress)
        {   Debug.Log("SCREEN TRANSITION STARTING");
            StartCoroutine(Screentransition());
        }
        else // To avoid spam clicking
        {   Debug.Log("SCREEN TRANSITION CANNOT BE STARTED YET");
            return;
        }
    }

    public void spriteHandler()
    {   
        frontScreen = arcadeMachine.transform.GetChild(0).gameObject;
        sideScreen = arcadeMachine.transform.GetChild(1).gameObject;

        currentSpriteRenderer = arcadeMachine.GetComponent<SpriteRenderer>();
        currentSprite = currentSpriteRenderer.sprite;
        
        //sideScreenRenderer = sideScreen.GetComponent<SpriteRenderer>();
        //frontScreenRenderer = frontScreen.GetComponent<SpriteRenderer>();

        if (currentSprite.name.Contains("1")) // 1 = side
        {   
            sideScreen.SetActive(true);
            frontScreen.SetActive(false);
            arcadeScreen = sideScreen;
        }
        else 
        {
            sideScreen.SetActive(false);
            frontScreen.SetActive(true);
            arcadeScreen = frontScreen;            
        }
        screenRenderer = arcadeScreen.GetComponent<SpriteRenderer>();
    }

    public void ScreenReset()
    {       
            round = 1;
            screenRenderer.enabled = false;

            screenRenderer.material.SetColor("_ScreenColorA", colors[2]);
            screenRenderer.material.SetColor("_ScreenColorB", colors[0]);
            screenRenderer.material.SetFloat("_VoronoiSpeedA", voronoiSpeeds[1]);
            screenRenderer.material.SetFloat("_VoronoiSpeedB", voronoiSpeeds[0]);
            screenRenderer.material.SetFloat("_GradientNoiseSpeedA", gradientNoiseSpeeds[1]);
            screenRenderer.material.SetFloat("_GradientNoiseSpeedB", gradientNoiseSpeeds[0]);
            screenRenderer.material.SetFloat("_WhiteNoiseEffect", 1);
    }

    void Start()
    {
        //shaderMat = arcadeScreen.GetComponent<Material>();
        arcadeScreen = sideScreen;
        screenRenderer = arcadeScreen.GetComponent<SpriteRenderer>();
        ScreenReset();
        arcadeScreen = frontScreen;
        screenRenderer = arcadeScreen.GetComponent<SpriteRenderer>();
        ScreenReset();
    }

    private IEnumerator Screentransition()
    {
        float elapsedTime = 0F;
        screenRenderer.enabled = true;
        transitionInProgress = true;
        
        if (round < 5) 
        {   
            if (round > 1) // round > 1 so there is no transition in the first round
            {
                while (elapsedTime < transitionDuration) // For smooth transition
                {
                    elapsedTime += Time.deltaTime; // Keeps track how much time has passed

                    voronoiSpeedA = Mathf.Lerp(voronoiSpeeds[round - 1], voronoiSpeeds[round], elapsedTime / transitionDuration);
                    screenRenderer.material.SetFloat("_VoronoiSpeedA", voronoiSpeedA);
                    voronoiSpeedB = Mathf.Lerp(voronoiSpeeds[round - 2], voronoiSpeeds[round - 1], elapsedTime / transitionDuration);
                    screenRenderer.material.SetFloat("_VoronoiSpeedB", voronoiSpeedB);

                    screenColorA = Color.Lerp(colors[round], colors[round + 1], elapsedTime / transitionDuration);
                    screenRenderer.material.SetColor("_ScreenColorA", screenColorA);
                    screenColorB = Color.Lerp(colors[round - 2], colors[round - 1], elapsedTime / transitionDuration);
                    screenRenderer.material.SetColor("_ScreenColorB", screenColorB);

                    gradientNoiseSpeedA = Mathf.Lerp(gradientNoiseSpeeds[round - 1], gradientNoiseSpeeds[round], elapsedTime / transitionDuration);
                    screenRenderer.material.SetFloat("_GradientNoiseSpeedA", gradientNoiseSpeedA);
                    gradientNoiseSpeedB = Mathf.Lerp(gradientNoiseSpeeds[round - 2], gradientNoiseSpeeds[round - 1], elapsedTime / transitionDuration);
                    screenRenderer.material.SetFloat("_GradientNoiseSpeedB", gradientNoiseSpeedB);

                    yield return null;
                }
            }
            round++;

            yield return new WaitForSeconds(1);
        }

        else if (round == 5) //White noise screen
        {
            Debug.Log("WHITE NOISE SCREEN, ROUND 5");
            screenRenderer.material.SetFloat("_WhiteNoiseEffect", 200);
            screenRenderer.material.SetColor("_ScreenColorA", new Color(1f, 1f, 1f, 1f) );
            screenRenderer.material.SetColor("_ScreenColorB", new Color(0.01f, 0.01f, 0.01f, 1f) );
            round++;
        }
        
        else //restart values & disable arcade screen
        {
            ScreenReset();
        }
        transitionInProgress = false;
        Debug.Log("SCREEN TRANSITION has been set to FALSE");
    }
}
}