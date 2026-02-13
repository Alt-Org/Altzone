using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
public class KaikkiOnHyvinMirrorScript : MonoBehaviour, ISoulHomeObjectClick
{

    public AudioSource audioSource;
    private float startingPitch = 3;
    public int round = 0;

    public void HandleClick()
    {
        Debug.Log("IN HANDLECLICK()");
        audioSource.Play(0); // 
    }

    public void pitchShift()
    {
        audioSource.pitch = startingPitch
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

}
}
