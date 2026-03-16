using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinMirrorScript : MonoBehaviour, ISoulHomeObjectClick
{
    private SpriteRenderer mirrorRenderer;
    [SerializeField] 
    private AudioSource audioSource;
    private float startingPitch = 3;
    private float currentAngle = 0;
    private int round = 0;
    private float transitionDuration = 3.0F;
    private bool active = false;

    public void HandleClick()
    {
        if (active)
        {return;}

        pitchShift();
        StartCoroutine(frameShake());
        if (round == 1) { mirrorRenderer.color = Color.HSVToRGB(1f, 0f, 1f);}
        Debug.Log("ROUND" + round);
        Debug.Log("AUDIO PITCH" + audioSource.pitch);
        audioSource.Play(0); // play the audio in the audio source component 
    }

    public void pitchShift()
    {   
        active = true;

        if (round < 5)
        {
            audioSource.pitch = startingPitch - round; // pitch lowers through audio source component 
            if (round == 4) {StartCoroutine(colorShift());}
            round++;
        }
        else
        {
            audioSource.pitch = startingPitch;
            round = 1; // 1 not 0, or else pitch = 3 twice in a row
        }
    }

    private IEnumerator colorShift()
    {
        float elapsedTime = 0f;
        float value = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            value = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);

            mirrorRenderer.color = Color.HSVToRGB(1f, 0f, value);

            yield return null;
        }
    }

    private IEnumerator frameShake()
    {
        float elapsedTime = 0f;
        float A = 5.0f;     // amplitude changes intensity
        float x = 0f;
        float w = 3.0f;     // higher ammount = frequency grows
        float p = 1.3f;     // slides function left or right
        float b = 1.0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;

            currentAngle = A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p);    // damped sine wave
            transform.eulerAngles = new Vector3(0, 0, currentAngle);   

            yield return null;
        }
        active = false;
        yield return null;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mirrorRenderer = GetComponent<SpriteRenderer>();
    }
}
}
