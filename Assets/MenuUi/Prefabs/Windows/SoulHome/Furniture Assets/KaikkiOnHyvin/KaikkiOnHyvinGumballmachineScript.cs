using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinGumballmachineScript : MonoBehaviour, ISoulHomeObjectClick
{
    private ParticleSystem partSys;
    // Start is called before the first frame update
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
        int randomNumber = Random.Range(0, messages.Length);
        message = messages[randomNumber];
        Debug.Log("RANDOM MESSAGE: " + message);
    }

    public void try1()
    {   
       // Debug.Log("IN TRY1");
       // partSys = GetComponent<ParticleSystem>();
       // partSys.Clear();
       // partSys.Stop();

       // var partSysMain = partSys.main;
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