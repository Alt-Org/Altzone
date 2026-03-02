using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinGumballmachineScript : MonoBehaviour, ISoulHomeObjectClick
{
    public ParticleSystem partSys;
    private float transitionDuration = 4.0F;
    private float newScaleY;
    private float newScaleX;

    private float addition;
    private bool active = false;


    private string[] messages = 
    {
        "1a","2a",
        "3a","4a",
        "5a","6a"
    };
    string message = "";
    
    void Start()
    {
        //try1();
    }

    public void HandleClick()
    {   
        
        if (active)
        {return;}

        int randomNumber = Random.Range(0, messages.Length);
        message = messages[randomNumber];

        Debug.Log("TRANSFORM POSITION " + this.transform.position);
        Debug.Log("TRANSFORM POSITION Y " + this.transform.position.y);
        Debug.Log("TRANSFORM SCALE " + this.transform.localScale);
        Debug.Log("TRANSFORM SCALE Y " + this.transform.localScale.y);
        //StartCoroutine(animation());
        //try1();
        StartCoroutine(animation2());
        Debug.Log("RANDOM MESSAGE: " + message);
    }

    private IEnumerator animation()
    {   
        active = true;
        float scaleX = this.transform.localScale.x;
        float scaleY = this.transform.localScale.y;
        float scaleZ = this.transform.localScale.z;
        float elapsedTime = 0f;
        float w = 6.3f; 
        float x = 0f;
        float p = 1.5f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            x += Time.deltaTime;
            newScaleY = scaleY + Mathf.Cos(w * x + p);
            

            transform.localScale = new Vector3(scaleX, newScaleY, scaleZ);

            yield return null;
        }
        active = false;
    }

        private IEnumerator animation2()
    {
        active = true;
        float elapsedTime = 0f;
        var partSysVOL = partSys.velocityOverLifetime;

        float scaleX = this.transform.localScale.x;
        float scaleY = this.transform.localScale.y;
        float scaleZ = this.transform.localScale.z;
        float MinVOL = 1.0f;
        float MaxVOL = 4.0f;

        float A = 3.0f;     // amplitude changes intensity
        float x = 0f;
        float w = 4.0f;     // higher ammount = frequency grows
        float p = 1.3f;     // slides function left or right
        float b = 4.0f;
        Debug.Log("IN ANIMATION 2");
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            x += Time.deltaTime;

            
            newScaleY = scaleY + (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            newScaleX = scaleX - (A * Mathf.Exp(-b * x) * Mathf.Cos(w * x + p));
            transform.localScale = new Vector3(newScaleX, newScaleY, scaleZ);

            addition = 100 * Mathf.Exp(-2 * x) * Mathf.Cos(w * x + 4.3f);
            Debug.Log("ADDITION:" + addition);

            partSysVOL.orbitalZ = new ParticleSystem.MinMaxCurve(1.0f + addition, 4.0f + addition);

            yield return null;
        }
        active = false;
    }



    public void try1()
    {   
       // Debug.Log("IN TRY1");
       //partSys = GetComponentInChildren<ParticleSystem>();
       // partSys.Clear();
       // partSys.Stop();

       var partSysVOL = partSys.velocityOverLifetime;
       partSysVOL.orbitalZ = new ParticleSystem.MinMaxCurve(70.0f, 100.0f);
       // var partSysVOL = partSys.velocityOverLifetime;
        //partSysMain.startColor = Color.red;
        //partSysVOL.orbitalZ = 22.0f;

       // partSys.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}